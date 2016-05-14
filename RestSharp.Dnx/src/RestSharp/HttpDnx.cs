//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace RestSharp
//{
//    public class HttpDnx : IHttp, IHttpFactory
//    {
//        private HttpClient client;

//        public IHttp Create()
//        {
//            return new HttpDnx();
//        }

//        /// <summary>
//        /// Default constructor
//        /// </summary>
//        public HttpDnx()
//        {
//            Headers = new List<HttpHeader>();
//            Files = new List<HttpFile>();
//            Parameters = new List<HttpParameter>();
//            Cookies = new List<HttpCookie>();
//            this.restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase);

//            this.AddSharedHeaderActions();
//            this.AddSyncHeaderActions();
//        }

//        public bool AlwaysMultipartFormData
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public CookieContainer CookieContainer
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public IList<HttpCookie> Cookies
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public ICredentials Credentials
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public Encoding Encoding
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public IList<HttpFile> Files
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public bool FollowRedirects
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public IList<HttpHeader> Headers
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public IList<HttpParameter> Parameters
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public bool PreAuthenticate
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public int ReadWriteTimeout
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public string RequestBody
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public byte[] RequestBodyBytes
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public string RequestContentType
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public Action<Stream> ResponseWriter
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public int Timeout
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public Uri Url
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public bool UseDefaultCredentials
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public string UserAgent
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }

//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public HttpWebRequest AsGetAsync(Action<HttpResponse> action, string httpMethod)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest AsPostAsync(Action<HttpResponse> action, string httpMethod)
//        {
//            throw new NotImplementedException();
//        }

        

//        public HttpWebRequest DeleteAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest GetAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest HeadAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest MergeAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest OptionsAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest PatchAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest PostAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }

//        public HttpWebRequest PutAsync(Action<HttpResponse> action)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
