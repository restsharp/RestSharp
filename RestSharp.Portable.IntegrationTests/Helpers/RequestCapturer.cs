using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.IntegrationTests
{
    /// <summary>
    /// A simple class used to capture and store request data 
    /// on the server so it can be reported back ot the test
    /// </summary>
    public class RequestCapturer
    {
        public static NameValueCollection Headers { get; set; }

        public static void Capture(HttpListenerContext context)
        {
            var request = context.Request;
            Headers = request.Headers;
        }
    }
}
