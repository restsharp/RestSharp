using System;
using System.Net;

namespace RestSharp.Extensions
{
    public static class ResponseStatusExtensions
    {
        /// <summary>
        ///     Convert a <see cref="ResponseStatus" /> to a <see cref="WebException" /> instance.
        /// </summary>
        /// <param name="responseStatus">The response status.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">responseStatus</exception>
        public static WebException ToWebException(this ResponseStatus responseStatus)
            => responseStatus switch
            {
                ResponseStatus.None => new WebException(
                    "The request could not be processed.",
                    WebExceptionStatus.ServerProtocolViolation
                ),
                ResponseStatus.Error => new WebException(
                    "An error occurred while processing the request.",
                    WebExceptionStatus.ServerProtocolViolation
                ),
                ResponseStatus.TimedOut => new WebException("The request timed-out.", WebExceptionStatus.Timeout),
                ResponseStatus.Aborted  => new WebException("The request was aborted.", WebExceptionStatus.Timeout),
                _                       => throw new ArgumentOutOfRangeException(nameof(responseStatus))
            };
    }
}