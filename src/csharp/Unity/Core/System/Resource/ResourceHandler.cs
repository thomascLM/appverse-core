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
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Core.System.Server.Net;

namespace Unity.Core.System.Resource
{
	public class ResourceHandler : IHttpHandler
	{
		private ApplicationSource appSource = ApplicationSource.DB_EMBEDDED; // default case: "unity.db" is embeded inside 
		public ApplicationSource AppSource { get { return appSource; } set { appSource = value; } }

		// Reads a file, and substitutes <%x>
		//HttpRequest req;
		//bool substitute = true;

		//public bool Substitute { get { return substitute; } set { substitute = value; } }

		public static Hashtable MimeTypes;

		static ResourceHandler ()
		{
			MimeTypes = new Hashtable ();
			MimeTypes [".html"] = "text/html";
			MimeTypes [".htm"] = "text/html";
			MimeTypes [".css"] = "text/css";
			MimeTypes [".js"] = "text/javascript";

			MimeTypes [".png"] = "image/png";
			MimeTypes [".gif"] = "image/gif";
			MimeTypes [".jpg"] = "image/jpeg";
			MimeTypes [".jpeg"] = "image/jpeg";
			MimeTypes [".tiff"] = "image/tiff";
			MimeTypes [".tif"] = "image/tiff";
			MimeTypes [".bmp"] = "image/bmp";

			MimeTypes [".xml"] = "application/xml";
			
			MimeTypes [".pdf"] = "application/pdf";
			
			MimeTypes [".json"] = "application/json";
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ResourceHandler ()
		{
		}

		/// <summary>
		/// Constructor to specify a non-default application source.
		/// </summary>
		/// <param name="_appSource">Application Source.</param>
		public ResourceHandler (ApplicationSource _appSource)
		{
			this.appSource = _appSource;
		}
		
		protected virtual string GetContentFromStreamReader (string filePath)
		{
			StreamReader sr = new StreamReader (filePath);
			string content = sr.ReadToEnd ();
			sr.Close ();
			return content;
		}
		
		protected virtual byte[] GetRawContentFromFileStream (string filePath)
		{
			FileStream fs = File.Open (filePath, FileMode.Open);
			byte[] buf = new byte[fs.Length];
			fs.Read (buf, 0, buf.Length);
			fs.Close ();
			return buf;
		}
		
		public virtual bool Process (HttpServer server, HttpRequest request, HttpResponse response)
		{

			if (this.appSource == ApplicationSource.FILE) {   // RESOURCES ARE SERVED FROM FILESYSTEM.

				string fn = server.GetFilename (request);
				if (!File.Exists (fn)) {
					response.ReturnCode = 404;
					response.Content = "File not found.";
					SystemLogger.Log (SystemLogger.Module .CORE, "# ResourceHandler. Error getting response content for [" + fn + "]: File Not Found");
					return true;
				}

				string ext = Path.GetExtension (fn);
				string mime = (string)MimeTypes [ext];
				if (mime == null)
					mime = "application/octet-stream";
				response.ContentType = mime;
				
				try {   // Only perform substitutions on HTML files
					
					//if (substitute && (mime.StartsWith("text/html"))) //(mime.Substring(0, 5) == "text/")
					if (mime.StartsWith ("text/html")) { //(mime.Substring(0, 5) == "text/")
						// Mime type 'text' is substituted
						response.Content = GetContentFromStreamReader (fn);
						/*
                        // Do substitutions
                        Regex regex = new Regex(@"\<\%(?<tag>[^>]+)\>");
                        lock (this)
                        {
                            req = request;
                            response.Content = regex.Replace(response.Content, new MatchEvaluator(RegexMatch));
                        }
                        */
					} else {
						response.RawContent = GetRawContentFromFileStream (fn);
					}
				} catch (Exception e) {
					response.ReturnCode = 500;
					response.Content = "Error reading file: " + e;
					SystemLogger.Log (SystemLogger.Module .CORE, "# ResourceHandler. Error getting response content for [" + fn + "]", e);
					return true;
				}
			

			} else if (this.appSource == ApplicationSource.DB_FILE) {   // RESOURCES ARE SERVED FROM A PRE_BUILD "DB" FILE.
				// TODO get resources from DB file.

			} else if (this.appSource == ApplicationSource.DB_EMBEDDED) {   // RESOURCES ARE SERVED FROM A PRE_BUILD "DB" FILE.
				// TODO get resources from embedded DB file.

			}

			return true;
		}

		public virtual string GetValue (HttpRequest req, string tag)
		{
			return "<span class=error>Unknown substitution: " + tag + "</span>";
		}
		/*
        string RegexMatch(Match m)
        {
            try
            {
                return GetValue(req, m.Groups["tag"].Value);
            }
            catch (Exception e)
            {
                SystemLogger.Log(SystemLogger.Module .CORE, "RegexMatch", e);
				return "<span class=error>Error substituting " + m.Groups["tag"].Value + "</span>";
            }
        }
        */
	}
}
