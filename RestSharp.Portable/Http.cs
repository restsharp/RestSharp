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
using System.Threading;

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

        /// <summary>
        /// Execute an async GET-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <param name="token">A task cancellation token</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsGetAsync(HttpMethod httpMethod, CancellationToken token)
        {
            return await MakeRequestAsync(httpMethod, token);
        }

        /// <summary>
        /// Execute an async POST-style request with the specified HTTP Method.  
        /// </summary>
        /// <param name="httpMethod">The HTTP method to execute.</param>
        /// <param name="token">A task cancellation token</param>
        /// <returns></returns>
        public async Task<HttpResponse> AsPostAsync(HttpMethod httpMethod, CancellationToken token)
        {
            return await MakeRequestAsync(httpMethod, token);
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
        private async Task<HttpResponse> MakeRequestAsync(HttpMethod method, CancellationToken token)
        {          
            this._handler.Configure(this._request);
            this._message.Configure(method, this._request);
            this._client.Configure(this._request);

            var httpResponse = new HttpResponse();

            //token.ThrowIfCancellationRequested();

            try
            {
                var responseMessage = await this._client.Instance.SendAsync(this._message.Instance, token);
                await httpResponse.ConvertFromResponseMessage(responseMessage);
            }
            //catch (InvalidOperationException exc)
            //{
            //    // Happens if an invalid URL is provided

            //    // NOTE: It should not really even be possible to get here
            //    // since internally we the UrlBuilder builds a proper URL

            //    httpResponse.ErrorMessage = exc.Message;
            //    httpResponse.ErrorException = exc;
            //    httpResponse.ResponseStatus = ResponseStatus.Error;
            //}
            catch (HttpRequestException exc)
            {
                // Happens if the DNS lookup fails, or request times out naturally
                // Here we try to return the inner exception which is generally more useful

                if (exc.InnerException != null)
                {
                    httpResponse.ErrorMessage = exc.InnerException.Message;
                    httpResponse.ErrorException = exc.InnerException;
                    httpResponse.ResponseStatus = ResponseStatus.Error;
                }
                else
                {
                    httpResponse.ErrorMessage = exc.Message;
                    httpResponse.ErrorException = exc;
                    httpResponse.ResponseStatus = ResponseStatus.Error;
                }
            }
            catch (TaskCanceledException exc)
            {
                // Happens if the user sets a timeout which expires OR
                // if the task is canceled.  We need to test to see which 
                // caused the exception

                if (exc.CancellationToken.IsCancellationRequested)
                {
                    httpResponse.ErrorMessage = exc.Message;
                    httpResponse.ErrorException = exc;
                    httpResponse.ResponseStatus = ResponseStatus.Cancelled;
                }
                else
                {
                    httpResponse.ErrorMessage = exc.Message;
                    httpResponse.ErrorException = exc;
                    httpResponse.ResponseStatus = ResponseStatus.TimedOut;
                }
            }

            //catch (Exception exc)
            //{
            //    // Catch all.  If this ever gets called we should really 
            //    // add a new case for it above.  In those cases maybe we really should 
            //    // just let this bubble out.
            //    httpResponse.ErrorMessage = exc.Message;
            //    httpResponse.ErrorException = exc;
            //    httpResponse.ResponseStatus = ResponseStatus.Error;
            //}

            return httpResponse;
        }

        #endregion

    }
}