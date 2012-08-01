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
using System.Text;
using Unity.Core.System.Server.Net;

namespace Unity.Core.System.Service
{
	public class ServiceURIHandler : IHttpHandler
	{
		private static string SERVICE_URI = "/service/";
		private IServiceLocator serviceLocator = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_serviceLocator"></param>
		public ServiceURIHandler (IServiceLocator _serviceLocator)
		{
			serviceLocator = _serviceLocator;
		}

        #region Miembros de IHttpHandler

		public bool Process (HttpServer server, HttpRequest request, HttpResponse response)
		{
			SystemLogger.Log (SystemLogger.Module .CORE, " ############## " + this.GetType () + " -> " + request.Url);
			if (request.Url.StartsWith (SERVICE_URI)) {
				SystemLogger.Log (SystemLogger.Module .CORE, "Service protocol.");
				try {
					string commandParams = request.Url.Substring (SERVICE_URI.Length);
					string[] commandParamsArray = commandParams.Split (new char[] { '/' });
					string serviceName = commandParamsArray [0];
                   
					Object service = serviceLocator.GetService (serviceName);
					byte[] result = null;
					string methodName = commandParamsArray [1];
					
					if (request.Method == "GET") {
						string[] methodParams = null;
						if (commandParamsArray.Length > 2) {
							methodParams = new string[commandParamsArray.Length - 2];
							for (int i = 2; i < commandParamsArray.Length; i++) {
								methodParams [i - 2] = commandParamsArray [i];
							}
						}
						result = getInvocationManager (request.Method).InvokeService (service, methodName, methodParams);
					} else if (request.Method == "POST") {
						string queryString = null;
						if (request.QueryString != null && request.QueryString.Length > "json=".Length) {
							//queryString = ServiceURIHandler.UrlDecode(request.QueryString.Substring("json=".Length));
							queryString = request.QueryString.Substring ("json=".Length);
						}
						result = getInvocationManager (request.Method).InvokeService (service, methodName, queryString);
					}
					
					response.RawContent = result;
					if (response.RawContent == null) {
						response.Content = "null";
						/* SERVICES COULD SEND NULL OBJETS.
						SystemLogger.Log(SystemLogger.Module .CORE, "No content available for request [" + request.Url + "," + request.Method + "]. Continue to next handler...");
                        return false;
                        */
					}
					response.ContentType = "application/json";
					return true;
				} catch (Exception e) {
					SystemLogger.Log (SystemLogger.Module .CORE, "Exception when parsing request [" + request.Url + "]", e);
					response.Content = "Malformed request.";
					return false;
				}
			} else {
				SystemLogger.Log (SystemLogger.Module .CORE, "Non service protocol. Continue to next handler...");
				return false;
			}
		}

        #endregion

		/// <summary>
		/// UrlDecodes a string without requiring System.Web
		/// </summary>
		/// <param name="text">String to decode.</param>
		/// <returns>decoded string</returns>
		public static string UrlDecode (string text)
		{
			// pre-process for + sign space formatting since System.Uri doesn't handle it
			// plus literals are encoded as %2b normally so this should be safe
			text = text.Replace ("+", " ");
			return Uri.UnescapeDataString (text);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reqMethod">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="IInvocationManager"/>
		/// </returns>
		private IInvocationManager getInvocationManager (string reqMethod)
		{
			if (reqMethod == "POST") {
				return new ServiceInvocationManager ();	
			} else {
				return new ResourceInvocationManager ();
			}
		}
	}
}
