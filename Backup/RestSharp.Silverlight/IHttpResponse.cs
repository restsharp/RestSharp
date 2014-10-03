using System;
using System.Collections.Generic;
using System.Net;

namespace RestSharp
{
	/// <summary>
	/// HTTP response data
	/// </summary>
	public interface IHttpResponse
	{
		/// <summary>
		/// MIME content type of response
		/// </summary>
		string ContentType { get; set; }

		/// <summary>
		/// Length in bytes of the response content
		/// </summary>
		long ContentLength { get; set; }

		/// <summary>
		/// Encoding of the response content
		/// </summary>
		string ContentEncoding { get; set; }

		/// <summary>
		/// String representation of response content
		/// </summary>
		string Content { get; }

		/// <summary>
		/// HTTP response status code
		/// </summary>
		HttpStatusCode StatusCode { get; set; }

		/// <summary>
		/// Description of HTTP status returned
		/// </summary>
		string StatusDescription { get; set; }

		/// <summary>
		/// Response content
		/// </summary>
		byte[] RawBytes { get; set; }

		/// <summary>
		/// The URL that actually responded to the content (different from request if redirected)
		/// </summary>
		Uri ResponseUri { get; set; }

		/// <summary>
		/// HttpWebResponse.Server
		/// </summary>
		string Server { get; set; }

		/// <summary>
		/// Headers returned by server with the response
		/// </summary>
		IList<HttpHeader> Headers { get; }

		/// <summary>
		/// Cookies returned by server with the response
		/// </summary>
		IList<HttpCookie> Cookies { get; }

		/// <summary>
		/// Status of the request. Will return Error for transport errors.
		/// HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
		/// </summary>
		ResponseStatus ResponseStatus { get; set; }

		/// <summary>
		/// Transport or other non-HTTP error generated while attempting request
		/// </summary>
		string ErrorMessage { get; set; }

		/// <summary>
		/// Exception thrown when error is encountered.
		/// </summary>
		Exception ErrorException { get; set; }
	}
}