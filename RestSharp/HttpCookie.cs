using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	/// <summary>
	/// Representation of an HTTP cookie
	/// </summary>
	public class HttpCookie
	{
		/// <summary>
		/// Name of the parameter
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Value of the parameter
		/// </summary>
		public string Value { get; set; }
	}
}
