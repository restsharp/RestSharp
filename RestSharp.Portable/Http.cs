#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RestSharp.Extensions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace RestSharp
{
	/// <summary>
	/// HttpWebRequest wrapper
	/// </summary>
	public partial class Http : IHttp //, IHttpFactory
    {
        //public IMessageHandlerFactory HandlerFactory = new SimpleMessageHandlerFactory<DefaultMessageHandler>();

        #region Private Members

        private IMessageHandler _handler;
        private IRequestMessage _message;
        private IHttpClient _client;
        private IHttpRequest _request;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Http(IHttpRequest request)
        {
            var handler = new DefaultMessageHandler();
            var message = new DefaultRequestMessage();
            var client = new HttpClientWrapper(handler);
            
            Configure(request, handler, message, client);
        }

        internal Http(IHttpRequest request, IMessageHandler handler, IRequestMessage message, IHttpClient client)
        {           
            Configure(request, handler, message, client);
        }

        #endregion

        #region Public Methods

        ///<summary>
        /// Creates an IHttp
        ///</summary>
        ///<returns></returns>
        //public IHttp Create()
        //{
        //    return new Http();
        //}

        public async Task<HttpResponse> DeleteAsync()
        {
            return await MakeRequestAsync(HttpMethod.Delete);
            
        }

        public async Task<HttpResponse> GetAsync()
        {
            return await MakeRequestAsync(HttpMethod.Get);
        }

        public async Task<HttpResponse> HeadAsync()
        {
            return await MakeRequestAsync(HttpMethod.Head);
        }

        public async Task<HttpResponse> OptionsAsync()
        {
            return await MakeRequestAsync(HttpMethod.Options);
        }

        public async Task<HttpResponse> PostAsync()
        {
            return await MakeRequestAsync(HttpMethod.Post);
        }

        public async Task<HttpResponse> PutAsync()
        {
            return await MakeRequestAsync(HttpMethod.Put);
        }

        public async Task<HttpResponse> PatchAsync()
        {
            return await MakeRequestAsync(new HttpMethod("PATCH"));
        }

        /// <summary>
        /// Execute an async GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsGetAsync(HttpMethod httpMethod)
        {
            return await MakeRequestAsync(httpMethod);
        }

        /// <summary>
        /// Execute an async POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsPostAsync(HttpMethod httpMethod)
        {
            return await MakeRequestAsync(httpMethod);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configure the class with the necessarily default stuff
        /// </summary>
        /// <param name="request"></param>
        /// <param name="handler"></param>
        /// <param name="message"></param>
        /// <param name="client"></param>
        private void Configure(IHttpRequest request, IMessageHandler handler, IRequestMessage message, IHttpClient client)
        {
            _handler = handler;
            _message = message;
            _client = client;
            _request = request;
        }

        /// <summary>
        /// Make requests to using the specified HTTP verb
        /// </summary>
        /// <param name="method">The HTTP method used to make the request</param>
        /// <returns></returns>
        private async Task<HttpResponse> MakeRequestAsync(HttpMethod method)
        {          
            this._handler.Configure(this._request);
            this._message.Configure(method, this._request);
            this._client.Configure(this._request);           

            var httpResponse = new HttpResponse();

            await this._client.Instance.SendAsync(this._message.Instance).ContinueWith(async t => {
                //TODO: Checking Faulted Handles DNS resolution failures.  Not sure about timeouts
                if (t.IsFaulted)
                {
                    httpResponse.ErrorMessage = t.Exception.Message;
                    httpResponse.ErrorException = t.Exception;
                    httpResponse.ResponseStatus = ResponseStatus.Error;
                }
                else if (t.IsCanceled) {
                    //httpResponse.ResponseStatus = ResponseStatus.None;
                }
                else {
                    await httpResponse.ConvertFromResponseMessage(t.Result);
                }

            });

            return httpResponse;
        }

        #endregion

    }
}