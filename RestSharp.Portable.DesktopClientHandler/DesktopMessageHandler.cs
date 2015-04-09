using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RestSharp.Portable.DesktopHandler
{
    public class DesktopMessageHandler : DefaultMessageHandler
    {
        private WebRequestHandler _instance;

        /// <summary>
        /// X509CertificateCollection to be sent with request
        /// </summary>
        public X509CertificateCollection ClientCertificates { get; set; }

        public override HttpClientHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WebRequestHandler();
                }
                return _instance;
            }

        }

        public override void Configure(IHttpRequest request)
        {
            base.Configure(request);

            ((WebRequestHandler)this.Instance).ClientCertificates.AddRange(ClientCertificates);
        }
    }
}
