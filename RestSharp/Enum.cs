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

namespace RestSharp
{
	public enum ParameterType
	{
		GetOrPost,
		UrlSegment,
		HttpHeader,
		RequestBody
	}

	public enum ResponseFormat
	{
		Auto,
		Json,
		Xml
	}

	public enum RequestFormat
	{
		Json,
		Xml
	}
	public enum Method
	{
		GET,
		POST,
		PUT,
		DELETE,
		HEAD,
		OPTIONS
	}

	public enum UrlMode
	{
		AsIs,
		ReplaceValues
	}

	public struct DateFormats
	{
		public const string Iso8601 = "s";
		public const string RoundTrip = "u";
	}

	public enum ResponseStatus
	{
		None,
		Success,
		Error
	}
}
