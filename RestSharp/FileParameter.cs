using System;
using System.IO;

namespace RestSharp
{
	/// <summary>
	/// Container for files to be uploaded with requests
	/// </summary>
	public class FileParameter
	{
		///<summary>
		/// Creates a file parameter from an array of bytes.
		///</summary>
		///<param name="name">The parameter name to use in the request.</param>
		///<param name="data">The data to use as the file's contents.</param>
		///<param name="filename">The filename to use in the request.</param>
		///<param name="contentType">The content type to use in the request.</param>
		///<returns>The <see cref="FileParameter"/></returns>
		public static FileParameter Create(string name, byte[] data, string filename, string contentType)
		{
#if FRAMEWORK
			var length = data.LongLength;
#else
			var length = (long)data.Length;
#endif
			return new FileParameter
			{
				Writer = s => s.Write(data, 0, data.Length),
				FileName = filename,
				ContentType = contentType,
				ContentLength = length,
				Name = name
			};
		}

		///<summary>
		/// Creates a file parameter from an array of bytes.
		///</summary>
		///<param name="name">The parameter name to use in the request.</param>
		///<param name="data">The data to use as the file's contents.</param>
		///<param name="filename">The filename to use in the request.</param>
		///<returns>The <see cref="FileParameter"/> using the default content type.</returns>
		public static FileParameter Create(string name, byte[] data, string filename)
		{
			return Create(name, data, filename, null);
		}
		
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
