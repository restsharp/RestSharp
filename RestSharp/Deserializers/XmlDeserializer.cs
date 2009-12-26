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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using RestSharp.Extensions;

namespace RestSharp.Deserializers
{
	public class XmlDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public DateFormat DateFormat { get; set; }

		public X Deserialize<X>(string content) where X : new() {
			if (content == null)
				return default(X);

			var doc = XDocument.Parse(content);
			var root = doc.Root;
			if (RootElement.HasValue())
				root = doc.Root.Element(RootElement.AsNamespaced(Namespace));

			// autodetect xml namespace
			if (!Namespace.HasValue()) {
				RemoveNamespace(doc);
			}

			var x = new X();
			var objType = x.GetType();

			if (objType.IsSubclassOfRawGeneric(typeof(List<>))) {
				x = (X)HandleListDerivative(x, root, objType.Name, objType);
			}
			else {
				Map(x, root);
			}

			return x;
		}

		void RemoveNamespace(XDocument xdoc) {
			foreach (XElement e in xdoc.Root.DescendantsAndSelf()) {
				if (e.Name.Namespace != XNamespace.None) {
					e.Name = XNamespace.None.GetName(e.Name.LocalName);
				}
				if (e.Attributes().Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None)) {
					e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
				}
			}
		}

		private void Map(object x, XElement root) {
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props) {
				var type = prop.PropertyType;

				if (!type.IsPublic || !prop.CanWrite)
					continue;

				var name = prop.Name.AsNamespaced(Namespace);
				var value = GetValueFromXml(root, name);

				if (value == null)
					continue;

				if (type.IsPrimitive) {
					prop.SetValue(x, Convert.ChangeType(value, type), null);
				}
				else if (type == typeof(string)) {
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(DateTime)) {
					value = value != null ? DateTime.Parse(value.ToString()) : default(DateTime);
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(Decimal)) {
					value = value != null ? Decimal.Parse(value.ToString()) : default(decimal);
					prop.SetValue(x, value, null);
				}
				else if (type.IsGenericType) {
					var t = type.GetGenericArguments()[0];
					var list = (IList)Activator.CreateInstance(type);

					var elements = root.Descendants().Where(d => d.Name.LocalName.RemoveUnderscores() == t.Name);
					foreach (var element in elements) {
						var item = CreateAndMap(t, element);
						list.Add(item);
					}

					prop.SetValue(x, list, null);
				}
				else if (type.IsSubclassOfRawGeneric(typeof(List<>))) {
					// handles classes that derive from List<T>
					// e.g. a collection that also has attributes
					var list = HandleListDerivative(x, root, prop.Name, type);
					prop.SetValue(x, list, null);
				}
				else {
					// nested property classes
					if (root != null) {
						var element = GetElementByName(root, name);
						if (element != null) {
							var item = CreateAndMap(type, element);
							prop.SetValue(x, item, null);
						}
					}
				}
			}
		}

		private object HandleListDerivative(object x, XElement root, string propName, Type type) {
			var t = type.BaseType.GetGenericArguments()[0];

			var list = (IList)Activator.CreateInstance(type);

			var elements = root.Descendants(t.Name.AsNamespaced(Namespace));

			foreach (var element in elements) {
				var item = CreateAndMap(t, element);
				list.Add(item);
			}

			// get properties too, not just list items
			Map(list, root.Element(propName.AsNamespaced(Namespace)));

			return list;
		}

		private object CreateAndMap(Type t, XElement element) {
			var item = Activator.CreateInstance(t);
			Map(item, element);
			return item;
		}

		private object GetValueFromXml(XElement root, string name) {
			return GetValueFromXml(root, XName.Get(name));
		}

		private object GetValueFromXml(XElement root, XName name) {
			object val = null;

			if (root != null) {
				var element = GetElementByName(root, name);
				if (element == null) {
					var attribute = GetAttributeByName(root, name);
					if (attribute != null) {
						val = attribute.Value;
					}
				}
				else {
					val = element.Value;
				}
			}

			return val;
		}

		private XElement GetElementByName(XElement root, XName name) {
			var lowerName = XName.Get(name.LocalName.ToLower(), name.NamespaceName);
			var camelName = XName.Get(name.LocalName.ToCamelCase(), name.NamespaceName);

			if (root.Element(name) != null) {
				return root.Element(name);
			}
			else if (root.Element(lowerName) != null) {
				return root.Element(lowerName);
			}
			else if (root.Element(camelName) != null) {
				return root.Element(camelName);
			}
			else if (name == "Value" && root.Value != null) {
				return root;
			}
			else {
				// try looking for element that matches sanitized property name
				var element = root.Descendants().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscores() == name.LocalName);
				if (element != null) {
					return element;
				}
			}

			return null;
		}

		private XAttribute GetAttributeByName(XElement root, XName name) {
			var lowerName = XName.Get(name.LocalName.ToLower(), name.NamespaceName);
			var camelName = XName.Get(name.LocalName.ToCamelCase(), name.NamespaceName);

			if (root.Attribute(name) != null) {
				return root.Attribute(name);
			}
			else if (root.Attribute(lowerName) != null) {
				return root.Attribute(lowerName);
			}
			else if (root.Attribute(camelName) != null) {
				return root.Attribute(camelName);
			}
			else {
				// try looking for element that matches sanitized property name
				var element = root.Attributes().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscores() == name.LocalName);
				if (element != null) {
					return element;
				}
			}

			return null;
		}
	}
}
