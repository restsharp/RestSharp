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
using System.Net;
using RestSharp.Extensions;

namespace RestSharp
{
	/// <summary>
	/// Base class for common properties shared by RestResponse and RestResponse[[T]]
	/// </summary>
	public abstract class RestResponseBase
	{
		private string _content;

		/// <summary>
		/// Default constructor
		/// </summary>
		public RestResponseBase()
		{
			Headers = new List<Parameter>();
			Cookies = new List<RestResponseCookie>();
		}
		/// <summary>
		/// The RestRequest that was made to get this RestResponse
		/// </summary>
		/// <remarks>
		/// Mainly for debugging if ResponseStatus is not OK
		/// </remarks> 
		public IRestRequest Request { get; set; }
		/// <summary>
		/// MIME content type of response
		/// </summary>
		public string ContentType { get; set; }
		/// <summary>
		/// Length in bytes of the response content
		/// </summary>
		public long ContentLength { get; set; }
		/// <summary>
		/// Encoding of the response content
		/// </summary>
		public string ContentEncoding { get; set; }
		/// <summary>
		/// String representation of response content
		/// </summary>
		public string Content
		{
			get
			{
				if (_content == null)
				{
					_content = RawBytes.AsString();
				}
				return _content;
			}
			set
			{
				_content = value;
			}
		}
		/// <summary>
		/// HTTP response status code
		/// </summary>
		public HttpStatusCode StatusCode { get; set; }
		/// <summary>
		/// Description of HTTP status returned
		/// </summary>
		public string StatusDescription { get; set; }
		/// <summary>
		/// Response content
		/// </summary>
		public byte[] RawBytes { get; set; }
		/// <summary>
		/// The URL that actually responded to the content (different from request if redirected)
		/// </summary>
		public Uri ResponseUri { get; set; }
		/// <summary>
		/// HttpWebResponse.Server
		/// </summary>
		public string Server { get; set; }
		/// <summary>
		/// Cookies returned by server with the response
		/// </summary>
		public IList<RestResponseCookie> Cookies { get; protected internal set; }
		/// <summary>
		/// Headers returned by server with the response
		/// </summary>
		public IList<Parameter> Headers { get; protected internal set; }

		private ResponseStatus _responseStatus = ResponseStatus.None;
		/// <summary>
		/// Status of the request. Will return Error for transport errors.
		/// HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
		/// </summary>
		public ResponseStatus ResponseStatus
		{
			get
			{
				return _responseStatus;
			}
			set
			{
				_responseStatus = value;
			}
		}

		/// <summary>
		/// Transport or other non-HTTP error generated while attempting request
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// The exception thrown during the request, if any
		/// </summary>
		public Exception ErrorException { get; set; }
	}

	/// <summary>
	/// Container for data sent back from API including deserialized data
	/// </summary>
	/// <typeparam name="T">Type of data to deserialize to</typeparam>
	public class RestResponse<T> : RestResponseBase, IRestResponse<T>
	{
		/// <summary>
		/// Deserialized entity data
		/// </summary>
		public T Data { get; set; }

		public static explicit operator RestResponse<T>(RestResponse response)
		{
			return new RestResponse<T>
			{
				ContentEncoding = response.ContentEncoding,
				ContentLength = response.ContentLength,
				ContentType = response.ContentType,
				Cookies = response.Cookies,
				ErrorMessage = response.ErrorMessage,
				Headers = response.Headers,
				RawBytes = response.RawBytes,
				ResponseStatus = response.ResponseStatus,
				ResponseUri = response.ResponseUri,
				Server = response.Server,
				StatusCode = response.StatusCode,
				StatusDescription = response.StatusDescription,
				Request = response.Request
			};
		}
        
	}

	/// <summary>
	/// Container for data sent back from API
	/// </summary>
	public class RestResponse : RestResponseBase, IRestResponse
	{

	}
}
