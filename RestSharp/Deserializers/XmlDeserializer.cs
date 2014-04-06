#region License

//   Copyright 2010 John Sheehan
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

namespace RestSharp.Deserializers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Linq;

	using RestSharp.Extensions;

	public class XmlDeserializer : DeserializerBase, IDeserializer
	{
		#region Constructors and Destructors

		public XmlDeserializer()
		{
			this.Culture = CultureInfo.InvariantCulture;            
		}

		#endregion

		#region Public Properties

		public CultureInfo Culture { get; set; }

		public string DateFormat { get; set; }

		public string Namespace { get; set; }

		public string RootElement { get; set; }

		#endregion

		#region Public Methods and Operators

		public virtual T Deserialize<T>(IRestResponse response)
		{
			if (string.IsNullOrEmpty(response.Content))
			{
				return default(T);
			}

			var doc = XDocument.Parse(response.Content);
			var root = doc.Root;
			if (this.RootElement.HasValue() && doc.Root != null)
			{
				root = doc.Root.Element(this.RootElement.AsNamespaced(this.Namespace));
			}

			// autodetect xml namespace
			if (!this.Namespace.HasValue())
			{
				RemoveNamespace(doc);
			}

			var x = (T)this.CreateInstance(typeof(T));
			var objType = x.GetType();

			if (objType.IsSubclassOfRawGeneric(typeof(List<>)))
			{
				x = (T)this.HandleListDerivative(root, objType.Name, objType);
			}
			else
			{
				this.Map(x, root);
			}

			return x;
		}

		#endregion

		#region Methods

		protected virtual object CreateAndMap(Type t, XElement element)
		{
			object item;
			if (t == typeof(string))
			{
				item = element.Value;
			}
			else if (t.IsPrimitive)
			{
				item = element.Value.ChangeType(t, this.Culture);
			}
			else
			{
				item = Activator.CreateInstance(t);
				this.Map(item, element);
			}

			return item;
		}

		protected virtual XAttribute GetAttributeByName(XElement root, XName name)
		{
			var lowerName = name.LocalName.ToLower().AsNamespaced(name.NamespaceName);
			var camelName = name.LocalName.ToCamelCase(this.Culture).AsNamespaced(name.NamespaceName);

			if (root.Attribute(name) != null)
			{
				return root.Attribute(name);
			}

			if (root.Attribute(lowerName) != null)
			{
				return root.Attribute(lowerName);
			}

			if (root.Attribute(camelName) != null)
			{
				return root.Attribute(camelName);
			}

			// try looking for element that matches sanitized property name
			var element =
				root.Attributes().FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName);
			return element;
		}

		protected virtual XElement GetElementByName(XElement root, XName name)
		{
			var lowerName = name.LocalName.ToLower().AsNamespaced(name.NamespaceName);
			var camelName = name.LocalName.ToCamelCase(this.Culture).AsNamespaced(name.NamespaceName);

			if (root.Element(name) != null)
			{
				return root.Element(name);
			}

			if (root.Element(lowerName) != null)
			{
				return root.Element(lowerName);
			}

			if (root.Element(camelName) != null)
			{
				return root.Element(camelName);
			}

			if (name == "Value".AsNamespaced(name.NamespaceName))
			{
				return root;
			}

			// try looking for element that matches sanitized property name (Order by depth)
			var element =
				root.Descendants()
					.OrderBy(d => d.Ancestors().Count())
					.FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName)
				?? root.Descendants()
					   .OrderBy(d => d.Ancestors().Count())
					   .FirstOrDefault(d => d.Name.LocalName.RemoveUnderscoresAndDashes() == name.LocalName.ToLower());

			return element;
		}

		protected virtual object GetValueFromXml(XElement root, XName name, PropertyInfo prop)
		{
			object val = null;

			if (root != null)
			{
				var element = this.GetElementByName(root, name);
				if (element == null)
				{
					var attribute = this.GetAttributeByName(root, name);
					if (attribute != null)
					{
						val = attribute.Value;
					}
				}
				else
				{
					if (!element.IsEmpty || element.HasElements || element.HasAttributes)
					{
						val = element.Value;
					}
				}
			}

			return val;
		}

		protected virtual void Map(object x, XElement root)
		{
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props)
			{
				var type = prop.PropertyType;

				if (!type.IsPublic || !prop.CanWrite)
				{
					continue;
				}

				var name = prop.Name.AsNamespaced(this.Namespace);
				var value = this.GetValueFromXml(root, name, prop);

				if (value == null)
				{
					// special case for inline list items
					if (type.IsGenericType)
					{
						var genericType = type.GetGenericArguments()[0];
						var first = this.GetElementByName(root, genericType.Name);
						var list = (IList)Activator.CreateInstance(type);

						if (first != null)
						{
							if (root != null)
							{
								var elements = root.Elements(first.Name);
								this.PopulateListFromElements(genericType, elements, list);
							}
						}

						prop.SetValue(x, list, null);
					}

					continue;
				}

				// check for nullable and extract underlying type
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					// if the value is empty, set the property to null...
					if (string.IsNullOrEmpty(value.ToString()))
					{
						prop.SetValue(x, null, null);
						continue;
					}

					type = type.GetGenericArguments()[0];
				}

				if (type == typeof(bool))
				{
					var toConvert = value.ToString().ToLower();
					prop.SetValue(x, XmlConvert.ToBoolean(toConvert), null);
				}
				else if (type.IsPrimitive)
				{
					prop.SetValue(x, value.ChangeType(type, this.Culture), null);
				}
				else if (type.IsEnum)
				{
					var converted = type.FindEnumValue(value.ToString(), this.Culture);
					prop.SetValue(x, converted, null);
				}
				else if (type == typeof(Uri))
				{
					var uri = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
					prop.SetValue(x, uri, null);
				}
				else if (type == typeof(string))
				{
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(DateTime))
				{
					value = this.DateFormat.HasValue()
								? DateTime.ParseExact(value.ToString(), this.DateFormat, this.Culture)
								: DateTime.Parse(value.ToString(), this.Culture);

					prop.SetValue(x, value, null);
				}

#if !PocketPC
				else if (type == typeof(DateTimeOffset))
				{
					var toConvert = value.ToString();
					if (string.IsNullOrEmpty(toConvert))
					{
						continue;
					}

					DateTimeOffset deserializedValue;
					try
					{
						deserializedValue = XmlConvert.ToDateTimeOffset(toConvert);
						prop.SetValue(x, deserializedValue, null);
					}
					catch (Exception)
					{
						object result;
						if (TryGetFromString(toConvert, out result, type))
						{
							prop.SetValue(x, result, null);
						}
						else
						{
							// fallback to parse
							deserializedValue = DateTimeOffset.Parse(toConvert);
							prop.SetValue(x, deserializedValue, null);
						}
					}
				}

#endif
				else if (type == typeof(decimal))
				{
					value = decimal.Parse(value.ToString(), this.Culture);
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(Guid))
				{
					var raw = value.ToString();
					value = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(value.ToString());
					prop.SetValue(x, value, null);
				}
				else if (type == typeof(TimeSpan))
				{
					var timeSpan = XmlConvert.ToTimeSpan(value.ToString());
					prop.SetValue(x, timeSpan, null);
				}
				else if (type.IsGenericType)
				{
					var t = type.GetGenericArguments()[0];
					var list = (IList)Activator.CreateInstance(type);

					var container = this.GetElementByName(root, prop.Name.AsNamespaced(this.Namespace));

					if (container.HasElements)
					{
						var first = container.Elements().FirstOrDefault();
						var elements = container.Elements(first.Name);
						this.PopulateListFromElements(t, elements, list);
					}

					prop.SetValue(x, list, null);
				}
				else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
				{
					// handles classes that derive from List<T>
					// e.g. a collection that also has attributes
					var list = this.HandleListDerivative(root, prop.Name, type);
					prop.SetValue(x, list, null);
				}
				else
				{
					// fallback to type converters if possible
					object result;
					if (TryGetFromString(value.ToString(), out result, type))
					{
						prop.SetValue(x, result, null);
					}
					else
					{
						// nested property classes
						if (root == null)
						{
							continue;
						}

						var element = this.GetElementByName(root, name);
						if (element == null)
						{
							continue;
						}

						var item = this.CreateAndMap(type, element);
						prop.SetValue(x, item, null);
					}
				}
			}
		}

		private static void RemoveNamespace(XDocument xDocument)
		{
			if (xDocument.Root == null)
			{
				return;
			}

			foreach (XElement e in xDocument.Root.DescendantsAndSelf())
			{
				if (e.Name.Namespace != XNamespace.None)
				{
					e.Name = XNamespace.None.GetName(e.Name.LocalName);
				}

				if (e.Attributes().Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None))
				{
					e.ReplaceAttributes(
						e.Attributes()
							.Select(
								a =>
								a.IsNamespaceDeclaration
									? null
									: a.Name.Namespace != XNamespace.None
										  ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value)
										  : a));
				}
			}
		}

		private static bool TryGetFromString(string inputString, out object result, Type type)
		{
#if !SILVERLIGHT && !WINDOWS_PHONE && !PocketPC
			var converter = TypeDescriptor.GetConverter(type);
			if (converter.CanConvertFrom(typeof(string)))
			{
				result = converter.ConvertFromInvariantString(inputString);
				return true;
			}

			result = null;
			return false;
#else
			result = null;
			return false;
#endif
		}		

		private object HandleListDerivative(XElement root, string propName, Type type)
		{
			Type t = type.IsGenericType ? type.GetGenericArguments()[0] : type.BaseType.GetGenericArguments()[0];

			var list = (IList)Activator.CreateInstance(type);

			var elements = root.Descendants(t.Name.AsNamespaced(this.Namespace));

			var name = t.Name;

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(this.Namespace);
				elements = root.Descendants(lowerName);
			}

			if (!elements.Any())
			{
				var camelName = name.ToCamelCase(this.Culture).AsNamespaced(this.Namespace);
				elements = root.Descendants(camelName);
			}

			if (!elements.Any())
			{
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == name);
			}

			if (!elements.Any())
			{
				var lowerName = name.ToLower().AsNamespaced(this.Namespace);
				elements = root.Descendants().Where(e => e.Name.LocalName.RemoveUnderscoresAndDashes() == lowerName);
			}

			this.PopulateListFromElements(t, elements, list);

			// get properties too, not just list items
			// only if this isn't a generic type
			if (!type.IsGenericType)
			{
				// when using RootElement, the hierarchy is different
				this.Map(list, root.Element(propName.AsNamespaced(this.Namespace)) ?? root);
			}

			return list;
		}

		private void PopulateListFromElements(Type t, IEnumerable<XElement> elements, IList list)
		{
			foreach (var element in elements)
			{
				var item = this.CreateAndMap(t, element);
				list.Add(item);
			}
		}

		#endregion
	}
}