using System;
using System.Net;

namespace RestSharp
{
    public class DefaultProxy : IWebProxy
    {
        public Uri GetProxy(Uri destination) => null;

        public bool IsBypassed(Uri host) => true;

        public ICredentials Credentials { get; set; }
    }
}