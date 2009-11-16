using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;

namespace RestSharp.Deserializers
{
	public class XmlDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public DateFormat DateFormat { get; set; }

		public X Deserialize<X>(string content) where X : new() {
			var x = new X();

			var doc = XDocument.Parse(content);
			var root = doc.Root;
			if (RootElement.HasValue())
				root = doc.Root.Element(GetNamespacedName(RootElement));

			var objType = x.GetType();

			if (objType.IsSubclassOfRawGeneric(typeof(List<>))) {
				x = (X)HandleListDerivative(x, root, objType.Name, objType);
			}
			else {
				Map(x, root);
			}

			return x;
		}

		private XName GetNamespacedName(string name) {
			XName xName = name;

			if (Namespace.HasValue())
				xName = XName.Get(name, Namespace);

			return xName;
		}

		private void Map(object x, XElement root) {
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props) {
				var type = prop.PropertyType;

				if (!type.IsPublic || !prop.CanWrite)
					continue;

				var name = GetNamespacedName(prop.Name);
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
						var element = root.Descendants().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscores() == name.LocalName);
						if (element != null) {
							var item = CreateAndMap(type, element);
							prop.SetValue(x, item, null);
						}
					}
				}
			}
		}

		private object HandleListDerivative(object x, XElement root, string propName, Type type) {
			var t = type.BaseType.GetGenericArguments()[0]; // TODO: only works one level down

			var list = (IList)Activator.CreateInstance(type);

			var elements = root.Descendants(GetNamespacedName(t.Name));

			foreach (var element in elements) {
				var item = CreateAndMap(t, element);
				list.Add(item);
			}

			// get properties too, not just list items
			Map(list, root.Element(GetNamespacedName(propName)));

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
				if (root.Element(name) != null) {
					val = root.Element(name).Value;
				}
				else if (root.Attribute(name) != null) {
					val = root.Attribute(name).Value;
				}
				else if (name == "Data" && root.Value != null) {
					val = root.Value;
				}
				else {
					// try looking for element that matches sanitized property name
					var element = root.Descendants().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscores() == name.LocalName);
					if (element != null) {
						val = element.Value;
					}
					else {
						// check attributes that match sanitized name
						var attribute = root.Attributes().FirstOrDefault(a => a.Name.LocalName.RemoveUnderscores() == name.LocalName);
						if (attribute != null) {
							val = attribute.Value;
						}
					}
				}
			}

			return val;
		}
	}
}
