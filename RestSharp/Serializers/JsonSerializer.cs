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
using Newtonsoft.Json;

namespace RestSharp.Serializers
{
	// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
	public class JsonSerializer : ISerializer<string> // wanted JObject
	{
		public string Serialize(object obj) {
			var serializer = new Newtonsoft.Json.JsonSerializer {
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include,
				DefaultValueHandling = DefaultValueHandling.Include
			};

			using (var stringWriter = new StringWriter()) {
				using (var jsonTextWriter = new JsonTextWriter(stringWriter)) {
					jsonTextWriter.Formatting = Formatting.Indented;
					jsonTextWriter.QuoteChar = '"';

					serializer.Serialize(jsonTextWriter, obj);

					var result = stringWriter.ToString();
					return result;
				}
			}
		}

		public string DateFormat { get; set; } // Currently unused
		public string RootElement { get; set; } // Currently unused
		public string Namespace { get; set; } // NOT USED FOR JSON
	}
}

#region Acknowledgements
// Original JsonSerializer contributed by Daniel Crenna (@dimebrain)
#endregion