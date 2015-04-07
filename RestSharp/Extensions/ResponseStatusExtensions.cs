using System;
using System.Net;

namespace RestSharp.Extensions
{
    public static class ResponseStatusExtensions
    {
        /// <summary>
        /// Convert a <see cref="ResponseStatus"/> to a <see cref="WebException"/> instance.
        /// </summary>
        /// <param name="responseStatus">The response status.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">responseStatus</exception>
        public static WebException ToWebException(this ResponseStatus responseStatus)
        {
            switch (responseStatus)
            {
                case ResponseStatus.None:
                    return new WebException("The request could not be processed.",
#if !SILVERLIGHT
                        WebExceptionStatus.ServerProtocolViolation
#else
                        WebExceptionStatus.UnknownError
#endif
                        );

                case ResponseStatus.Error:
                    return new WebException("An error occurred while processing the request.",
#if !SILVERLIGHT
                        WebExceptionStatus.ServerProtocolViolation
#else
                        WebExceptionStatus.UnknownError
#endif
                        );

                case ResponseStatus.TimedOut:
                    return new WebException("The request timed-out.",
#if !SILVERLIGHT
                        WebExceptionStatus.Timeout
#else
                        WebExceptionStatus.UnknownError
#endif
                        );

                case ResponseStatus.Aborted:
                    return new WebException("The request was aborted.",
#if !SILVERLIGHT
                        WebExceptionStatus.Timeout
#else
                        WebExceptionStatus.RequestCanceled
#endif
                        );

                default:
                    throw new ArgumentOutOfRangeException("responseStatus");
            }
        }
    }
}
