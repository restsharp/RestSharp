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
using System.Text;

namespace RestSharp.Deserializers
{
	/// <summary>
	/// Wrapper for System.Xml.Serialization.XmlSerializer.
	/// </summary>
	public class DotNetXmlDeserializer : IDeserializer
	{
		public string DateFormat { get; set; }

		public string Namespace { get; set; }

		public string RootElement { get; set; }

		public T Deserialize<T>(IRestResponse response)
		{
			if (string.IsNullOrEmpty(response.Content))
			{
				return default(T);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response.Content)))
			{
				var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}
	}
}