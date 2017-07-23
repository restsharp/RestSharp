using System.Net;

namespace RestSharp
{
    public class RestRequestAsyncHandle
    {
        public HttpWebRequest WebRequest;

        public RestRequestAsyncHandle() { }

        public RestRequestAsyncHandle(HttpWebRequest webRequest)
        {
            this.WebRequest = webRequest;
        }

        public void Abort()
        {
            if (this.WebRequest != null)
            {
                this.WebRequest.Abort();
            }
        }
    }
}
