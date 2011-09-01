using System;
using System.Net;
namespace RestSharp
{
	public class RestRequestAsyncHandle
	{
		public HttpWebRequest WebRequest;
		
		public RestRequestAsyncHandle()
		{
		}
		
		public RestRequestAsyncHandle(HttpWebRequest webRequest)
		{
			WebRequest = webRequest;
		}
		
		public void Abort()
		{
			if (WebRequest != null)
				WebRequest.Abort();
		}
	}
}

