/*
 Copyright (c) 2012 GFT Appverse, S.L., Sociedad Unipersonal.

 This Source  Code Form  is subject to the  terms of  the Appverse Public License 
 Version 2.0  (“APL v2.0”).  If a copy of  the APL  was not  distributed with this 
 file, You can obtain one at http://www.appverse.mobi/licenses/apl_v2.0.pdf.

 Redistribution and use in  source and binary forms, with or without modification, 
 are permitted provided that the  conditions  of the  AppVerse Public License v2.0 
 are met.

 THIS SOFTWARE IS PROVIDED BY THE  COPYRIGHT HOLDERS  AND CONTRIBUTORS "AS IS" AND
 ANY EXPRESS  OR IMPLIED WARRANTIES, INCLUDING, BUT  NOT LIMITED TO,   THE IMPLIED
 WARRANTIES   OF  MERCHANTABILITY   AND   FITNESS   FOR A PARTICULAR  PURPOSE  ARE
 DISCLAIMED. EXCEPT IN CASE OF WILLFUL MISCONDUCT OR GROSS NEGLIGENCE, IN NO EVENT
 SHALL THE  COPYRIGHT OWNER  OR  CONTRIBUTORS  BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL,  SPECIAL,   EXEMPLARY,  OR CONSEQUENTIAL DAMAGES  (INCLUDING, BUT NOT
 LIMITED TO,  PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES;  LOSS OF USE, DATA, OR
 PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) 
 ARISING  IN  ANY WAY OUT  OF THE USE  OF THIS  SOFTWARE,  EVEN  IF ADVISED OF THE 
 POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Unity.Core.System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections;

namespace Unity.Core.IO
{
	public abstract class AbstractIO : IIo
	{
		private static string SERVICES_CONFIG_FILE = "app/config/io-services-config.xml";
		private static int DEFAULT_RESPONSE_TIMEOUT = 100000; // 100 seconds
		private IOServicesConfig servicesConfig = new IOServicesConfig ();  // empty list
		private static IDictionary<ServiceType, string> contentTypes = new Dictionary<ServiceType, string> ();
		private CookieContainer cookieContainer = null;
        
		static AbstractIO ()
		{
			contentTypes [ServiceType.XMLRPC_JSON] = "application/json";
			contentTypes [ServiceType.XMLRPC_XML] = "text/xml";
			contentTypes [ServiceType.REST_JSON] = "application/json";
			contentTypes [ServiceType.REST_XML] = "text/xml";
			contentTypes [ServiceType.SOAP_JSON] = "application/json";
			contentTypes [ServiceType.SOAP_XML] = "text/xml";
			contentTypes [ServiceType.AMF_SERIALIZATION] = "";
			contentTypes [ServiceType.REMOTING_SERIALIZATION] = "";
			contentTypes [ServiceType.OCTET_BINARY] = "application/octet-stream";
			contentTypes [ServiceType.GWT_RPC] = "text/x-gwt-rpc; charset=utf-8";

		}
		
		private string _servicesConfigFile = SERVICES_CONFIG_FILE;
		private string _IOUserAgent = "Unity 1.0";
		
		public virtual string IOUserAgent { 
			get {
				return this._IOUserAgent;
			} 
			set { 
				this._IOUserAgent = value; 
			}
		}
		
		public virtual string ServicesConfigFile { 
			get {
				return this._servicesConfigFile;
			} 
			set { 
				this._servicesConfigFile = value; 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AbstractIO ()
		{
			LoadServicesConfig ();
			this.cookieContainer = new CookieContainer ();
		}
		
		/// <summary>
		/// Default method, to be overrided by platform implementation. 
		/// </summary>
		/// <returns>
		/// A <see cref="Stream"/>
		/// </returns>
		public virtual Stream GetConfigFileStream ()
		{
			SystemLogger.Log (SystemLogger.Module .CORE, "# Loading IO Services Configuration from file: " + ServicesConfigFile);
			
			return new FileStream (ServicesConfigFile, FileMode.Open);
		}

		/// <summary>
		/// 
		/// </summary>
		protected void LoadServicesConfig ()
		{
			try {   // FileStream to read the XML document.
				Stream fs = GetConfigFileStream ();
				if (fs != null) {
					XmlSerializer serializer = new XmlSerializer (typeof(IOServicesConfig));
					servicesConfig = (IOServicesConfig)serializer.Deserialize (new MemoryStream (((MemoryStream)fs).GetBuffer ()));
				}
			} catch (Exception e) {
				SystemLogger.Log (SystemLogger.Module .CORE, "Error when loading services configuration", e);
			}
		}


        #region Miembros de IIo

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IOService[] GetServices ()
		{
			return servicesConfig.Services.ToArray ();
		}

		/// <summary>
		/// Get the IO Service that matches the given name.
		/// </summary>
		/// <param name="name">Service name to match.</param>
		/// <returns>IO Service.</returns>
		public IOService GetService (string name)
		{
			IOService service = null;
			IOService[] services = GetServices ();
			if (services != null) {
				foreach (IOService serv in services) {
					if (serv.Name == name) {
						service = serv;
						break;
					}
				}
			}
			return service;
		}

		/// <summary>
		/// Get the IO Service that matches the given name and type.
		/// </summary>
		/// <param name="name">Service name to match.</param>
		/// <param name="type">Service type to match.</param>
		/// <returns>IO Service.</returns>
		public IOService GetService (string name, ServiceType type)
		{
			IOService service = null;
			IOService[] services = GetServices ();
			if (services != null) {
				foreach (IOService serv in services) {
					if (serv.Name == name && serv.Type == type) {
						service = serv;
						break;
					}
				}
			}
			return service;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		public virtual IOResponse InvokeService (IORequest request, IOService service)
		{
			IOResponse response = new IOResponse ();

			if (service != null) {
				SystemLogger.Log (SystemLogger.Module .CORE, "Request content: " + request.Content);
				byte[] requestData = request.GetRawContent ();
				
				if (service.Endpoint == null) {
					SystemLogger.Log (SystemLogger.Module .CORE, "No endpoint configured for this service name: " + service.Name);
					return response;
				}

				string requestUriString = String.Format ("{0}:{1}{2}", service.Endpoint.Host, service.Endpoint.Port, service.Endpoint.Path);
				if (service.Endpoint.Port == 0) {
					requestUriString = String.Format ("{0}{1}", service.Endpoint.Host, service.Endpoint.Path);
				}
				
				if (service.RequestMethod == RequestMethod.GET && request.Content != null) {
					// add request content to the URI string when GET method.
					requestUriString += request.Content;
				}
				
				SystemLogger.Log (SystemLogger.Module .CORE, "Requesting service: " + requestUriString);
				try {
					
					ServicePointManager.ServerCertificateValidationCallback += delegate(object sender,X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
						SystemLogger.Log (SystemLogger.Module .CORE, "*************** On ServerCertificateValidationCallback: accept all certificates");
						return true;
					};
					
					HttpWebRequest req = (HttpWebRequest)WebRequest.Create (requestUriString);
					req.Method = service.RequestMethod.ToString (); // default is POST
					req.ContentType = contentTypes [service.Type];

					// check specific request ContentType defined, and override service type in that case
					if (request.ContentType != null && request.ContentType.Length > 0) {
						req.ContentType = request.ContentType;
					}
					SystemLogger.Log (SystemLogger.Module.CORE, "Request content type: " + req.ContentType);
					SystemLogger.Log (SystemLogger.Module.CORE, "Request method: " + req.Method);

					req.Accept = req.ContentType; // setting "Accept" header with the same value as "Content Type" header, it is needed to be defined for some services.
					req.ContentLength = request.GetContentLength ();
					SystemLogger.Log (SystemLogger.Module.CORE, "Request content length: " + req.ContentLength);
					req.Timeout = DEFAULT_RESPONSE_TIMEOUT; // in millisecods (default is 10 seconds)
					req.KeepAlive = false;
					
					// user agent needs to be informed - some servers check this parameter and send 500 errors when not informed.
					req.UserAgent = this.IOUserAgent;
					SystemLogger.Log (SystemLogger.Module.CORE, "Request UserAgent : " + req.UserAgent);
					
					// add specific headers to the request
					if (request.Headers != null && request.Headers.Length > 0) {
						foreach (IOHeader header in request.Headers) {
							req.Headers.Add (header.Name, header.Value);
							SystemLogger.Log (SystemLogger.Module.CORE, "Added request header: " + header.Name + "=" + req.Headers.Get (header.Name));
						}
					}

					// Assign the cookie container on the request to hold cookie objects that are sent on the response.
					// Required even though you no cookies are send.
					req.CookieContainer = this.cookieContainer;

					// add cookies to the request cookie container
					if (request.Session != null && request.Session.Cookies != null && request.Session.Cookies.Length > 0) {
						foreach (IOCookie cookie in request.Session.Cookies) {
							req.CookieContainer.Add (req.RequestUri, new Cookie (cookie.Name, cookie.Value));
							SystemLogger.Log (SystemLogger.Module.CORE, "Added cookie [" + cookie.Name + "] to request.");
						}
					}

					SystemLogger.Log (SystemLogger.Module.CORE, "HTTP Request cookies: " + req.CookieContainer.GetCookieHeader (req.RequestUri));

					if (service.Endpoint.ProxyUrl != null) {
						WebProxy myProxy = new WebProxy ();
						Uri proxyUri = new Uri (service.Endpoint.ProxyUrl);
						myProxy.Address = proxyUri;
						req.Proxy = myProxy;
					}
					
					if (req.Method == RequestMethod.POST.ToString ()) {
						// send data only for POST method.
						SystemLogger.Log (SystemLogger.Module.CORE, "Sending data on the request stream... (POST)");
						SystemLogger.Log (SystemLogger.Module.CORE, "request data length: " + requestData.Length);
						using (Stream requestStream = req.GetRequestStream()) {
							SystemLogger.Log (SystemLogger.Module.CORE, "request stream: " + requestStream);
							requestStream.Write (requestData, 0, requestData.Length);
						}
					}
	
					string result = null;
					byte[] resultBinary = null;
					
					string responseMimeTypeOverride = null;
					
					using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
						SystemLogger.Log (SystemLogger.Module.CORE, "getting response...");
						using (Stream stream = resp.GetResponseStream()) {
							SystemLogger.Log (SystemLogger.Module.CORE, "getting response stream...");
							if (ServiceType.OCTET_BINARY.Equals (service.Type)) {
								
								// TODO workaround to avoid problems when serving binary content (corrupted content)
								Thread.Sleep (500);
								
								int lengthContent = 0;
								if (resp.GetResponseHeader ("Content-Length") != null && resp.GetResponseHeader ("Content-Length") != "") {
									lengthContent = Int32.Parse (resp.GetResponseHeader ("Content-Length"));
								}
								if (lengthContent > 0) {
									// Read in block
									resultBinary = new byte[lengthContent];
									stream.Read (resultBinary, 0, lengthContent);
								} else {
									// Read to end of stream
									MemoryStream memBuffer = new MemoryStream ();
									byte[] readBuffer = new byte[256];
									int readLen = 0;
									do {
										readLen = stream.Read (readBuffer, 0, readBuffer.Length);
										memBuffer.Write (readBuffer, 0, readLen);
									} while (readLen >0);
									
									resultBinary = memBuffer.ToArray ();
									memBuffer.Close ();
									memBuffer = null;
								}
							} else {
								SystemLogger.Log (SystemLogger.Module.CORE, "reading response content...");
								using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
									result = reader.ReadToEnd ();
								}
							}
						}
						responseMimeTypeOverride = resp.GetResponseHeader ("Content-Type");

						// get response cookies (stored on cookiecontainer)
						if (response.Session == null) {
							response.Session = new IOSessionContext ();
                            
						}
						response.Session.Cookies = new IOCookie[this.cookieContainer.Count];
						IEnumerator enumerator = this.cookieContainer.GetCookies (req.RequestUri).GetEnumerator ();
						int i = 0;
						while (enumerator.MoveNext()) {
							Cookie cookieFound = (Cookie)enumerator.Current;
							SystemLogger.Log (SystemLogger.Module.CORE, "Found cookie on response: " + cookieFound.Name + "=" + cookieFound.Value);
							IOCookie cookie = new IOCookie ();
							cookie.Name = cookieFound.Name;
							cookie.Value = cookieFound.Value;
							response.Session.Cookies [i] = cookie;
							i++;
						}
					}
					
					if (ServiceType.OCTET_BINARY.Equals (service.Type)) {
						if (responseMimeTypeOverride != null && !responseMimeTypeOverride.Equals (contentTypes [service.Type])) {
							response.ContentType = responseMimeTypeOverride;
						} else {
							response.ContentType = contentTypes [service.Type];
						}
						response.ContentBinary = resultBinary; // Assign binary content here
					} else {
						response.ContentType = contentTypes [service.Type];
						response.Content = result;
					}

				} catch (WebException ex) {
					SystemLogger.Log (SystemLogger.Module .CORE, "WebException requesting service: " + requestUriString + ".", ex);
					response.ContentType = contentTypes [ServiceType.REST_JSON];
					response.Content = "WebException Requesting Service: " + requestUriString + ". Message: " + ex.Message;
				} catch (Exception ex) {
					SystemLogger.Log (SystemLogger.Module .CORE, "Unnandled Exception requesting service: " + requestUriString + ".", ex);
					response.ContentType = contentTypes [ServiceType.REST_JSON];
					response.Content = "Unhandled Exception Requesting Service: " + requestUriString + ". Message: " + ex.Message;
				} 
			} else {
				SystemLogger.Log (SystemLogger.Module .CORE, "Null service received for invoking.");
			}


			return response;
		}

		/// <summary>
		/// Invokes service, given its name, using the provided request.
		/// </summary>
		/// <param name="request">IO request.</param>
		/// <param name="serviceName">Service Name.</param>
		/// <returns>IO response.</returns>
		public IOResponse InvokeService (IORequest request, string serviceName)
		{
			return InvokeService (request, GetService (serviceName));
		}

		/// <summary>
		/// Invokes service, given its name and type, using the provided request.
		/// </summary>
		/// <param name="request">IO request.</param>
		/// <param name="serviceName"></param>
		/// <param name="type"></param>
		/// <returns>IO response.</returns>
		public IOResponse InvokeService (IORequest request, string serviceName, ServiceType type)
		{
			return InvokeService (request, GetService (serviceName, type));
		}

		public IOResponseHandle InvokeService (IORequest request, IOService service, IOResponseHandler handler)
		{
			throw new NotImplementedException ();
		}

		public IOResponseHandle InvokeService (IORequest request, string serviceName, IOResponseHandler handler)
		{
			throw new NotImplementedException ();
		}

		public IOResponseHandle InvokeService (IORequest request, string serviceName, ServiceType type, IOResponseHandler handler)
		{
			throw new NotImplementedException ();
		}

        #endregion
	}
}
