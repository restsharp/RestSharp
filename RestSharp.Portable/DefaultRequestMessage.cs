using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RestSharp
{
    public class DefaultRequestMessage : IRequestMessage
    {
        private readonly IDictionary<string, Action<HttpRequestMessage, IEnumerable<string>>> _restrictedHeaderActions;

        HttpRequestMessage _instance;

        public DefaultRequestMessage() {

            _restrictedHeaderActions = new Dictionary<string, Action<HttpRequestMessage, IEnumerable<string>>>(StringComparer.OrdinalIgnoreCase);

            AddSharedHeaderActions();
        }

        public DefaultRequestMessage(HttpMethod method, IHttpRequest request) : this()
        {
            Configure(method, request);
        }

        public HttpRequestMessage Instance
        {
            get { 
                if (_instance == null)
                {
                    _instance = new HttpRequestMessage();
                }
                return _instance;
            }
        }

        public void Configure(HttpMethod method, IHttpRequest request)
        {
            this.Instance.RequestUri = request.Url;
            this.Instance.Method = method;

            foreach (var header in request.Headers)
            {
                if (_restrictedHeaderActions.ContainsKey(header.Name))
                {
                    _restrictedHeaderActions[header.Name].Invoke(this.Instance, header.Value);
                }
                else
                {
                    this.Instance.Headers.Add(header.Name, header.Value);
                }
            }

            if (method == HttpMethod.Put || method == HttpMethod.Post || method.Method == "PATCH" || method == HttpMethod.Delete || method == HttpMethod.Options)
            {
                if (request.HasFiles || request.AlwaysMultipartFormData)
                {
                    var content = new MultipartFormDataContent(); //TODO: check to see if there is ever any reason to just use MultipartContent type?

                    foreach (var file in request.Files)
                    {
                        content.Add(new ByteArrayContent(file.Data), file.Name, file.FileName);
                    }

                    if (request.HasParameters) { content.Add(new FormUrlEncodedContent(request.Parameters)); }
                    if (request.HasBody) { content.Add(new StringContent(request.RequestBody)); }

                    this.Instance.Content = content;
                }
                else if (request.HasParameters)
                {
                    this.Instance.Content = new FormUrlEncodedContent(request.Parameters);
                }
                else if (request.HasBody)
                {
                    var content = new StringContent(request.RequestBody);
                            
                    if (!string.IsNullOrEmpty(request.RequestContentType))
                    {
                        //if not set, StringContent defaults to text/plain
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.RequestContentType);
                    }

                    this.Instance.Content = content;
                }
            }            
        }

        private void AddSharedHeaderActions()
        {
            _restrictedHeaderActions.Add("Date", (r, v) => { /* Set by system */ });
            _restrictedHeaderActions.Add("Host", (r, v) => { /* Set by system */ });
        }
    }
}
