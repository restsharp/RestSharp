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

using System.IO;
using Newtonsoft.Json.Linq;

namespace RestSharp.Extensions
{
	/// <summary>
	/// Extension method overload!
	/// </summary>
	public static class MiscExtensions
	{
#if !WINDOWS_PHONE
		/// <summary>
		/// Save a byte array to a file
		/// </summary>
		/// <param name="input">Bytes to save</param>
		/// <param name="path">Full path to save file to</param>
		public static void SaveAs(this byte[] input, string path)
		{
			File.WriteAllBytes(path, input);
		}
#endif

		/// <summary>
		/// Read a stream into a byte array
		/// </summary>
		/// <param name="input">Stream to read</param>
		/// <returns>byte[]</returns>
		public static byte[] ReadAsBytes(this Stream input)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Gets string value from JToken
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static string AsString(this JToken token)
		{
			return (string)token;
		}
	}
}