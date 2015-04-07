using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RestSharp
{
    public interface IHttpRequest
    {
        Action<Stream> ResponseWriter { get; set; }
        CookieContainer CookieContainer { get; set; }
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Always send a multipart/form-data request - even when no Files are present.
        /// </summary>
        bool AlwaysMultipartFormData { get; set; }

        string UserAgent { get; set; }
        int Timeout { get; set; }

        //		X509CertificateCollection ClientCertificates { get; set; }

        int? MaxAutomaticRedirects { get; set; }

        bool UseDefaultCredentials { get; set; }

        IList<HttpHeader> Headers { get; }
        IList<KeyValuePair<string, string>> Parameters { get; }
        IList<HttpFile> Files { get; }
        IList<HttpCookie> Cookies { get; }
        string RequestBody { get; set; }
        string RequestContentType { get; set; }

        /// <summary>
        /// An alternative to RequestBody, for when the caller already has the byte array.
        /// </summary>
        byte[] RequestBodyBytes { get; set; }

        Uri Url { get; set; }

        IWebProxy Proxy { get; set; }


        bool HasParameters { get; }
        bool HasCookies { get; }
        bool HasBody { get; }
        bool HasFiles { get;  }

    }
}
