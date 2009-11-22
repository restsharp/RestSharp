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

namespace RestSharp
{
	public class Http : IHttp
	{
		public ICredentials Credentials { get; set; }
		public IDictionary<string, string> Headers { get; private set; }
		public Http() {
			Headers = new Dictionary<string, string>();
		}

		public RestResponse Post(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return Post(uri, @params, "application/x-www-form-urlencoded");
		}

		public RestResponse Post(Uri uri, IEnumerable<KeyValuePair<string, string>> @params, string contentType) {
			return PostPutInternal(uri, @params, contentType, "POST");
		}

		public RestResponse Put(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return Put(uri, @params, "application/x-www-form-urlencoded");
		}

		public RestResponse Put(Uri uri, IEnumerable<KeyValuePair<string, string>> @params, string contentType) {
			return PostPutInternal(uri, @params, contentType, "PUT");
		}

		private RestResponse PostPutInternal(Uri uri, IEnumerable<KeyValuePair<string, string>> @params, string contentType, string method) {
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.Method = method;

			if (this.Credentials != null) {
				request.Credentials = this.Credentials;
			}

			if (@params.Count() > 1) {
				var data = EncodeParameters(@params);
				request.ContentLength = data.Length;
				request.ContentType = contentType;

				var requestStream = request.GetRequestStream();
				using (StreamWriter writer = new StreamWriter(requestStream, Encoding.ASCII)) {
					writer.Write(data);
				}
			}

			AppendHeaders(request);
			return GetResponse(request);
		}

		private string EncodeParameters(IEnumerable<KeyValuePair<string, string>> @params) {
			var querystring = new StringBuilder();
			foreach (var p in @params) {
				if (querystring.Length > 1) querystring.Append("&");
				querystring.AppendFormat("{0}={1}", HttpUtility.UrlEncode(p.Key), HttpUtility.UrlEncode(p.Value));
			}

			return querystring.ToString();
		}

		public RestResponse Get(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return GetStyleVerbInternal(uri, "GET", @params);
		}

		public RestResponse Head(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return GetStyleVerbInternal(uri, "HEAD", @params);
		}

		public RestResponse Options(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return GetStyleVerbInternal(uri, "OPTIONS", @params);
		}

		public RestResponse Delete(Uri uri, IEnumerable<KeyValuePair<string, string>> @params) {
			return GetStyleVerbInternal(uri, "DELETE", @params);
		}

		private RestResponse GetStyleVerbInternal(Uri uri, string method, IEnumerable<KeyValuePair<string, string>> @params) {
			string url = uri.ToString();
			if (@params.Count() > 1) {
				var data = EncodeParameters(@params);
				url = string.Format("{0}?{1}", url, data);
			}

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;
			if (this.Credentials != null) {
				request.Credentials = this.Credentials;
			}

			AppendHeaders(request);
			return GetResponse(request);
		}

		private void AppendHeaders(HttpWebRequest request) {
			foreach (var header in Headers) {
				request.Headers[header.Key] = header.Value;
			}
		}

		private static RestResponse GetResponse(HttpWebRequest request) {
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
