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
using System.Net;
using System.IO;

namespace RestSharp
{
	public interface IHttp
	{
		ICredentials Credentials { get; set; }
		string UserAgent { get; set; }
		int Timeout { get; set; }

		// TODO: move to HttpRequest
		IList<HttpHeader> Headers { get; }
		IList<HttpParameter> Parameters { get; }
		IList<HttpFile> Files { get; }
		IList<HttpCookie> Cookies { get; }
		string RequestBody { get; set; }
		string RequestContentType { get; set; }

		Uri Url { get; set; }

		void DeleteAsync(Action<HttpResponse> action);
		void GetAsync(Action<HttpResponse> action);
		void HeadAsync(Action<HttpResponse> action);
		void OptionsAsync(Action<HttpResponse> action);
		void PostAsync(Action<HttpResponse> action);
		void PutAsync(Action<HttpResponse> action);

#if FRAMEWORK
		HttpResponse Delete();
		HttpResponse Get();
		HttpResponse Head();
		HttpResponse Options();
		HttpResponse Post();
		HttpResponse Put();

		IWebProxy Proxy { get; set; }
#endif
	}
}