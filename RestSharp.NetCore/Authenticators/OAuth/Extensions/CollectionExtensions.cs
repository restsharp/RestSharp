using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            return new[] { item };
        }

        public static IEnumerable<T> And<T>(this T item, T other)
        {
            return new[] { item, other };
        }

        public static IEnumerable<T> And<T>(this IEnumerable<T> items, T item)
        {
            foreach (T i in items)
            {
                yield return i;
            }

            yield return item;
        }

        public static TK TryWithKey<T, TK>(this IDictionary<T, TK> dictionary, T key)
        {
            return dictionary.ContainsKey(key)
                ? dictionary[key]
                : default(TK);
        }

        public static IEnumerable<T> ToEnumerable<T>(this object[] items) where T : class
        {
            return items.Select(item => item as T);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
            {
                action(item);
            }
        }

#if !WINDOWS_PHONE && !SILVERLIGHT
        public static void AddRange(this IDictionary<string, string> collection, NameValueCollection range)
        {
            foreach (string key in range.AllKeys)
            {
                collection.Add(key, range[key]);
            }
        }

        public static string ToQueryString(this NameValueCollection collection)
        {
            StringBuilder sb = new StringBuilder();

            if (collection.Count > 0)
            {
                sb.Append("?");
            }

            int count = 0;

            foreach (string key in collection.AllKeys)
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
            StringBuilder sb = new StringBuilder();
            int total = collection.Count;
            int count = 0;

            foreach (WebPair item in collection)
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
