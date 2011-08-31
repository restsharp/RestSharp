using System;
using System.Net;
namespace RestSharp
{
	public class RestRequestAsyncHandle
	{
		public HttpWebRequest _webRequest;
		
		public RestRequestAsyncHandle()
		{
		}
		
		public RestRequestAsyncHandle(HttpWebRequest webRequest)
		{
			_webRequest = webRequest;
		}
		
		public void Abort()
		{
			if (_webRequest != null)
				_webRequest.Abort();
		}
	}
}

