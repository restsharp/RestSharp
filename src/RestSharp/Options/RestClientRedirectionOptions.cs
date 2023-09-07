using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace RestSharp.Options {
    [GenerateImmutable]
    public class RestClientRedirectionOptions {
        static readonly Version Version = new AssemblyName(typeof(RestClientOptions).Assembly.FullName!).Version!;

        public bool FollowRedirects { get; set; } = true;
        public bool FollowRedirectsToInsecure { get; set; } = false;
        public bool ForwardHeaders { get; set; } = true;
        public bool ForwardAuthorization { get; set; } = false;
        public bool ForwardCookies { get; set; } = true;
        public bool ForwardBody { get; set; } = true;
        public bool ForwardQuery { get; set; } = true;
        public int MaxRedirects { get; set; }
        public bool ForwardFragment { get; set; } = true;
        public IReadOnlyList<HttpStatusCode> RedirectStatusCodes { get; set; }

        public RestClientRedirectionOptions() {
            RedirectStatusCodes = new List<HttpStatusCode>() {
                HttpStatusCode.MovedPermanently,
                HttpStatusCode.SeeOther,
                HttpStatusCode.TemporaryRedirect,
                HttpStatusCode.Redirect,
    #if NET
                HttpStatusCode.PermanentRedirect,
    #endif
            }.AsReadOnly();
        }
    }
}
