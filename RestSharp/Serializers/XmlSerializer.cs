#region License
//   Copyright 2009 John Sheehan
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
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RestSharp.Extensions;

namespace RestSharp.Serializers
{
	public class XmlSerializer : ISerializer
	{
		public XDocument Serialize(object obj) {
			var doc = new XDocument();

			var t = obj.GetType();
			var props = t.GetProperties().Where(p => p.CanRead);
			var root = new XElement(t.Name);

			foreach (var prop in props) {
				AddElementForProperty(root, obj, prop);
			}

			if (RootElement.HasValue()) {
				var wrapper = new XElement(RootElement.AsNamespaced(Namespace), root);
				doc.Add(wrapper);
			}
			else {
				doc.Add(root);
			}

			return doc;
		}

		private void AddElementForProperty(XElement parent, object obj, PropertyInfo prop) {

			// make sure to use Namespaced name
			var name = prop.Name.AsNamespaced(Namespace);
			var useAttribute = false;
			object value = GetValue(prop.GetValue(obj, null));

			// check for [SerializeAs(Name="", Attribute=true)] options
			var settings = prop.GetAttribute<SerializeAsAttribute>();
			if (settings != null) {
				name = settings.Name.HasValue() ? settings.Name : name;
				useAttribute = settings.Attribute;
			}

			if (useAttribute) {
				if (value is XElement) {
					throw new InvalidOperationException("You cannot nest objects in properties serialized as attributes.");
				}
				parent.Add(new XAttribute(name, value));
			}
			else {
				parent.Add(new XElement(name, value)); 
			}

		}

		private object GetValue(object obj) {
			object output = obj;
			if (obj is DateTime) {
				// check for DateFormat when adding date props
				if (DateFormat != DateFormat.None) {
					output = ((DateTime)obj).ToString(DateFormat.GetFormatString());
				}
			}
			else {
				// handle nested types (recursively call AddElementForProperty)
				// handle List<T>
			}

			return output;
		}

		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public DateFormat DateFormat { get; set; } // Currently unused
	}
}