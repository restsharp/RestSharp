#region License
//   Copyright 2009 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using RestSharp.Extensions;

namespace RestSharp
{
	public class Http : IHttp
	{
		protected bool HasParameters {
			get {
				return Parameters.Any();
			}
		}

		protected bool HasFiles {
			get {
				return Files.Any();
			}
		}
	
		public ICredentials Credentials { get; set; }
		public IList<HttpFile> Files { get; private set; }
		public IList<HttpHeader> Headers { get; private set; }
		public IList<HttpParameter> Parameters { get; private set; }
		public IWebProxy Proxy { get; set; }

		public Uri Url { get; set; }

		public Http() {
			Headers = new List<HttpHeader>();
			Files = new List<HttpFile>();
			Parameters = new List<HttpParameter>();
		}

		public RestResponse Post() {
			return PostPutInternal("POST");
		}

		public RestResponse Put() {
			return PostPutInternal("PUT");
		}

		private RestResponse PostPutInternal(string method) {

			var request = (HttpWebRequest)WebRequest.Create(Url);
			request.Method = method;

			if (this.Credentials != null) {
				request.Credentials = this.Credentials;
			}

			if (this.Proxy != null) {
				request.Proxy = this.Proxy;
			}

			if (HasFiles) {
				var contentType = GetMultipartFormContentType();
				var data = GetMultipartFormData();
			}
			else {
				if (HasParameters) {
					var data = EncodeParameters();
					request.ContentLength = data.Length;
					request.ContentType = "application/x-www-form-urlencoded";

					var requestStream = request.GetRequestStream();
					using (StreamWriter writer = new StreamWriter(requestStream, Encoding.ASCII)) {
						writer.Write(data);
					}
				}
			}

			AppendHeaders(request);
			return GetResponse(request);
		}

		private string _formBoundary = "-----------------------------28947758029299";
		private string GetMultipartFormContentType() {
			return "multipart/form-data; boundary=" + _formBoundary;
		}

		private byte[] GetMultipartFormData() {
			var boundary = _formBoundary;
			var encoding = Encoding.ASCII;
			Stream formDataStream = new System.IO.MemoryStream();

			foreach (var file in Files) {
				var fileName = file.FileName;
				var data = file.Data;
				var length = data.Length;

				// Add just the first part of this param, since we will write the file data directly to the Stream
				string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n",
					boundary,
					fileName,
					"application/octet-stream"); // todo: allow this to be specified

				formDataStream.Write(encoding.GetBytes(header), 0, header.Length);

				// Write the file data directly to the Stream, rather than serializing it to a string.
				formDataStream.Write(data, 0, length);
				string lineEnding = "\r\n";
				formDataStream.Write(encoding.GetBytes(lineEnding), 0, lineEnding.Length);
			}

			//{
			//       string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
			//           boundary,
			//           param.Key,
			//           param.Value);
			//       formDataStream.Write(encoding.GetBytes(postData), 0, postData.Length);
			//   }

			// Add the end of the request
			string footer = "\r\n--" + boundary + "--\r\n";
			formDataStream.Write(encoding.GetBytes(footer), 0, footer.Length);

			// Dump the Stream into a byte[]
			formDataStream.Position = 0;
			byte[] formData = new byte[formDataStream.Length];
			formDataStream.Read(formData, 0, formData.Length);
			formDataStream.Close();

			return formData;
		}


		private string EncodeParameters() {
			var querystring = new StringBuilder();
			foreach (var p in Parameters) {
				if (querystring.Length > 1) querystring.Append("&");
				querystring.AppendFormat("{0}={1}", HttpUtility.UrlEncode(p.Name), HttpUtility.UrlEncode(p.Value));
			}

			return querystring.ToString();
		}

		public RestResponse Get() {
			return GetStyleVerbInternal("GET");
		}

		public RestResponse Head() {
			return GetStyleVerbInternal("HEAD");
		}

		public RestResponse Options() {
			return GetStyleVerbInternal("OPTIONS");
		}

		public RestResponse Delete() {
			return GetStyleVerbInternal("DELETE");
		}

		private RestResponse GetStyleVerbInternal(string method) {
			string url = Url.ToString();
			if (HasParameters) {
				var data = EncodeParameters();
				url = string.Format("{0}?{1}", url, data);
			}

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;

			if (this.Credentials != null) {
				request.Credentials = this.Credentials;
			}

			if (this.Proxy != null) {
				request.Proxy = this.Proxy;
			}

			AppendHeaders(request);
			return GetResponse(request);
		}

		private void AppendHeaders(HttpWebRequest request) {
			foreach (var header in Headers) {
				request.Headers[header.Name] = header.Value;
			}
		}

		private RestResponse GetResponse(HttpWebRequest request) {
			using (var raw = (HttpWebResponse)request.GetResponse()) {
				var response = new RestResponse();
				response.ContentType = raw.ContentType;
				response.ContentLength = raw.ContentLength;
				response.ContentEncoding = raw.ContentEncoding;
				response.Content = raw.GetResponseStream().ReadAsString();
				response.StatusCode = raw.StatusCode;
				response.StatusDescription = raw.StatusDescription;
				response.ResponseUri = raw.ResponseUri;
				response.Server = raw.Server;

				raw.Close();

				return response;
			}
		}
	}
}
