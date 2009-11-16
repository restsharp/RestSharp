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

using System;
using System.Collections.Generic;
using System.Net;

namespace RestSharp
{
	public interface IHttp
	{
		ICredentials Credentials { get; set; }
		RestResponse Delete(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		RestResponse Get(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		RestResponse Head(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		IDictionary<string, string> Headers { get; }
		RestResponse Options(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		RestResponse Post(Uri uri, IEnumerable<KeyValuePair<string, string>> @params, string contentType);
		RestResponse Post(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		RestResponse Put(Uri uri, IEnumerable<KeyValuePair<string, string>> @params);
		RestResponse Put(Uri uri, IEnumerable<KeyValuePair<string, string>> @params, string contentType);
	}
}