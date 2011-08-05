using System;
using System.Net;
namespace RestSharp
{
	public class RestRequestAsyncHandle
	{
		private HttpWebRequest _webRequest;
		
		public RestRequestAsyncHandle(HttpWebRequest webRequest)
		{
			_webRequest = webRequest;
		}
		
		public void Abort()
		{
			_webRequest.Abort();
		}
	}
}

