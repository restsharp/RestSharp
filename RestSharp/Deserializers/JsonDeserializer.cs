using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace RestSharp.Deserializers
{
	public class JsonDeserializer : IDeserializer
	{
		public string RootElement { get; set; }
		public string Namespace { get; set; }
		public DateFormat DateFormat { get; set; }

		public X Deserialize<X>(string content) where X : new() {
			var x = new X();

			JObject json = JObject.Parse(content);
			JToken root = json.Root;

			if (RootElement.HasValue())
				root = json[RootElement];

			Map(x, root);

			return x;
		}

		private void Map(object x, JToken json) {
			var objType = x.GetType();
			var props = objType.GetProperties();

			foreach (var prop in props) {
				var type = prop.PropertyType;

				if (!type.IsPublic || !prop.CanWrite)
					continue;

				var name = prop.Name;
				var value = json[name];

				if (value == null)
					continue;

				if (type.IsPrimitive) {
					prop.SetValue(x, Convert.ChangeType(value.ToString(), type), null);
				}
				else if (type == typeof(string)) {
					string raw = value.ToString();
					// remove leading and trailing "
					prop.SetValue(x, raw.Substring(1, raw.Length - 2), null);
				}
				else if (type == typeof(DateTime)) {
					var dt = value != null ? value.ToString().ParseJsonDate() : default(DateTime);
					prop.SetValue(x, dt, null);
				}
				else if (type == typeof(Decimal)) {
					var dec = value != null ? Decimal.Parse(value.ToString()) : default(decimal);
					prop.SetValue(x, dec, null);
				}
				else if (type.IsGenericType) {
					// TODO: handle Dictionaries

					var t = type.GetGenericArguments()[0];
					var list = (IList)Activator.CreateInstance(type);

					var elements = value.Children();

					foreach (var element in elements) {
						var item = CreateAndMap(t, element);
						list.Add(item);
					}

					prop.SetValue(x, list, null);
				}
				else {
					// nested property classes
					var item = CreateAndMap(type, json[name]);
					prop.SetValue(x, item, null);
				}
			}
		}

		private object CreateAndMap(Type t, JToken element) {
			var item = Activator.CreateInstance(t);
			Map(item, element);
			return item;
		}

	}
}