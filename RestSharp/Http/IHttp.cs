using System;
using System.Collections.Generic;
using System.Net;

using RestSharp;

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