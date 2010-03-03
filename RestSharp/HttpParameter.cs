using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	/// <summary>
	/// Representation of an HTTP parameter (QueryString or Form value)
	/// </summary>
	public class HttpParameter
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
