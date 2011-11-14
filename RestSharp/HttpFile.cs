using System;
using System.IO;

namespace RestSharp
{
	/// <summary>
	/// Container for HTTP file
	/// </summary>
	public class HttpFile
	{

		/// <summary>
		/// The length of data to be sent
		/// </summary>
		public long ContentLength { get; set; }
		/// <summary>
		/// Provides raw data for file
		/// </summary>
		public Action<Stream> Writer { get; set; }
		/// <summary>
		/// Name of the file to use when uploading
		/// </summary>
		public string FileName { get; set; }
		/// <summary>
		/// MIME content type of file
		/// </summary>
		public string ContentType { get; set; }
		/// <summary>
		/// Name of the parameter
		/// </summary>
		public string Name { get; set; }
	}
}
