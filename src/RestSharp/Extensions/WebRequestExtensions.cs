using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Extensions
{
    public static class WebRequestExtensions
    {
        public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest, CancellationToken cancellationToken)
            => Task.Run(
                () =>
                    Task<Stream>.Factory.FromAsync(
                        (callback, state) => ((WebRequest) state).BeginGetRequestStream(callback, state),
                        iar => ((WebRequest) iar.AsyncState).EndGetRequestStream(iar),
                        webRequest
                    ), cancellationToken
            );

        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest, CancellationToken cancellationToken)
            => Task.Run(
                () =>
                    Task<WebResponse>.Factory.FromAsync(
                        (callback, state) => ((WebRequest) state).BeginGetResponse(callback, state),
                        iar => ((WebRequest) iar.AsyncState).EndGetResponse(iar),
                        webRequest
                    ), cancellationToken
            );
    }
}