#region License
//   Copyright 2010 John Sheehan
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
using System.Linq;
using System.Net;
using System.IO;
using RestSharp.Serializers;

namespace RestSharp
{
	public class RestRequest
	{
		public RestRequest() {
			Parameters = new List<Parameter>();
			Files = new List<FileParameter>();
		}

		public RestRequest(Method verb)
			: this() {
			Verb = verb;
		}

		public RestRequest(string action)
			: this() {
			Action = action;
		}

		public RestRequest(string action, Method verb)
			: this() {
			Action = action;
			Verb = verb;
		}

		public RestRequest AddFile(string path) {
			string fileName = Path.GetFileName(path);
			var file = File.ReadAllBytes(path);

			return AddFile(file, fileName);
		}

		public RestRequest AddFile(byte[] bytes, string fileName) {
			return AddFile(bytes, fileName, null);
		}

		public RestRequest AddFile(byte[] bytes, string fileName, string contentType) {
			Files.Add(new FileParameter { Data = bytes, FileName = fileName, ContentType = contentType });
			return this;
		}

		public RestRequest AddBody(object obj, string xmlNamespace) {
			var xml = new XmlSerializer(xmlNamespace);
			return AddParameter("", xml.Serialize(obj).ToString(), ParameterType.RequestBody);
		}

		public RestRequest AddBody(object obj) {
			return AddBody(obj, "");
		}

		public RestRequest AddObject(object obj, params string[] whitelist) {
			// automatically create parameters from object props
			var type = obj.GetType();
			var props = type.GetProperties();

			foreach (var prop in props) {
				bool isAllowed = whitelist.Length > 0 && whitelist.Contains(prop.Name);

				if (isAllowed) {
					var propType = prop.PropertyType;
					var val = prop.GetValue(obj, null);

					if (val != null) {
						if (propType.IsArray) {
							val = string.Join(",", (string[])val);
						}

						AddParameter(prop.Name, val);
					}
				}
			}

			return this;
		}

		public RestRequest AddObject(object obj) {
			AddObject(obj, string.Empty);
			return this;
		}

		public RestRequest AddParameter(Parameter p) {
			Parameters.Add(p);
			return this;
		}

		public RestRequest AddParameter(string name, object value) {
			return AddParameter(new Parameter { Name = name, Value = value, Type = ParameterType.GetOrPost });
		}

		public RestRequest AddParameter(string name, object value, ParameterType type) {
			return AddParameter(new Parameter { Name = name, Value = value, Type = type });
		}

		public List<Parameter> Parameters { get; private set; }
		public List<FileParameter> Files { get; private set; }

		private Method _verb = Method.GET;
		public Method Verb {
			get { return _verb; }
			set { _verb = value; }
		}

		public string Action { get; set; }

		private string _ActionFormat;
		public string ActionFormat {
			get {
				return _ActionFormat;
			}
			set {
				UrlMode = UrlMode.ReplaceValues;
				_ActionFormat = value;
			}
		}

		private UrlMode _UrlMode = UrlMode.AsIs;
		private UrlMode UrlMode {
			get {
				return _UrlMode;
			}
			set {
				_UrlMode = value;
			}
		}

		public string BaseUrl { get; set; }
		public string ContentType { get; set; }

		private ResponseFormat _ResponseFormat = ResponseFormat.Auto;
		public ResponseFormat ResponseFormat {
			get {
				return _ResponseFormat;
			}
			set {
				_ResponseFormat = value;
			}
		}

		public string RootElement { get; set; }
		public string XmlNamespace { get; set; }
		public ICredentials Credentials { get; set; }

		public Uri GetUri() {
			Uri url = null;

			switch (UrlMode) {
				case UrlMode.AsIs:
					url = new Uri(string.Format("{0}/{1}", BaseUrl, Action));
					break;
				case UrlMode.ReplaceValues:
					string assembled = this.ActionFormat;
					var urlParms = Parameters.Where(p => p.Type == ParameterType.UrlSegment);
					foreach (var p in urlParms) {
						assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString());
					}

					url = new Uri(string.Format("{0}/{1}", BaseUrl, assembled));
					break;
			}

			return url;
		}
	}
}
