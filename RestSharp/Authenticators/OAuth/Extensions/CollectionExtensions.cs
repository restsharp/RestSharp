using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
	internal static class CollectionExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>(this T item)
		{
			return new[] {item};
		}

		public static IEnumerable<T> And<T>(this T item, T other)
		{
			return new[] {item, other};
		}

		public static IEnumerable<T> And<T>(this IEnumerable<T> items, T item)
		{
			foreach (var i in items)
			{
				yield return i;
			}

			yield return item;
		}

		public static K TryWithKey<T, K>(this IDictionary<T, K> dictionary, T key)
		{
			return dictionary.ContainsKey(key) ? dictionary[key] : default(K);
		}

		public static IEnumerable<T> ToEnumerable<T>(this object[] items) where T : class
		{
			foreach (var item in items)
			{
				var record = item as T;
				yield return record;
			}
		}

		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}

#if !WINDOWS_PHONE && !SILVERLIGHT

		public static void AddRange(this IDictionary<string, string> collection, NameValueCollection range)
		{
			foreach (var key in range.AllKeys)
			{
				collection.Add(key, range[key]);
			}
		}

		public static string ToQueryString(this NameValueCollection collection)
		{
			var sb = new StringBuilder();
			if (collection.Count > 0)
			{
				sb.Append("?");
			}

			var count = 0;
			foreach (var key in collection.AllKeys)
			{
				sb.AppendFormat("{0}={1}", key, collection[key].UrlEncode());
				count++;

				if (count >= collection.Count)
				{
					continue;
				}
				sb.Append("&");
			}
			return sb.ToString();
		}

#endif

		public static string Concatenate(this WebParameterCollection collection, string separator, string spacer)
		{
			var sb = new StringBuilder();

			var total = collection.Count;
			var count = 0;

			foreach (var item in collection)
			{
				sb.Append(item.Name);
				sb.Append(separator);
				sb.Append(item.Value);

				count++;
				if (count < total)
				{
					sb.Append(spacer);
				}
			}

			return sb.ToString();
		}
	}
}