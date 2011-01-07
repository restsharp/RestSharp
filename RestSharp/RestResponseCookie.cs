using System;
namespace RestSharp
{
	public class RestResponseCookie
	{
		/// <summary>
		/// Comment of the cookie
		/// </summary>
		public string Comment { get; set; }
		/// <summary>
		/// Comment of the cookie
		/// </summary>
		public Uri CommentUri { get; set; }
		/// <summary>
		/// Indicates whether the cookie should be discarded at the end of the session
		/// </summary>
		public bool Discard { get; set; }
		/// <summary>
		/// Domain of the cookie
		/// </summary>
		public string Domain { get; set; }
		/// <summary>
		/// Indicates whether the cookie is expired
		/// </summary>
		public bool Expired { get; set; }
		/// <summary>
		/// Date and time that the cookie expires
		/// </summary>
		public DateTime Expires { get; set; }
		/// <summary>
		/// Indicates that this cookie should only be accessed by the server
		/// </summary>
		public bool HttpOnly { get; set; }
		/// <summary>
		/// Name of the cookie
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Path of the cookie
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// Port of the cookie
		/// </summary>
		public string Port { get; set; }
		/// <summary>
		/// Indicates that the cookie should only be sent over secure channels
		/// </summary>
		public bool Secure { get; set; }
		/// <summary>
		/// Date and time the cookie was created
		/// </summary>
		public DateTime TimeStamp { get; set; }
		/// <summary>
		/// Value of the cookie
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// Version of the cookie
		/// </summary>
		public int Version { get; set; }
	}
}

