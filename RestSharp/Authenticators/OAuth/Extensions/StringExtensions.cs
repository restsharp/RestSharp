using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrBlank(this string value) => string.IsNullOrEmpty(value) ||
                                                               !string.IsNullOrEmpty(value) &&
                                                               value.Trim() == string.Empty;

        public static bool EqualsIgnoreCase(this string left, string right) => 
            string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;

        public static bool EqualsAny(this string input, params string[] args) => 
            args.Aggregate(false, (current, arg) => current | input.Equals(arg));

        public static string FormatWith(this string format, params object[] args) => 
            string.Format(format, args);

        public static string FormatWithInvariantCulture(this string format, params object[] args) => 
            string.Format(CultureInfo.InvariantCulture, format, args);

        public static string Then(this string input, string value) => string.Concat(input, value);

        public static string UrlEncode(this string value) => Uri.EscapeDataString(value);

        public static string UrlDecode(this string value) => Uri.UnescapeDataString(value);

        public static Uri AsUri(this string value) => new Uri(value);

        public static string ToBase64String(this byte[] input) => Convert.ToBase64String(input);

        public static byte[] GetBytes(this string input) => Encoding.UTF8.GetBytes(input);

        public static string PercentEncode(this string s)
        {
            var bytes = s.GetBytes();
            var sb = new StringBuilder();

            foreach (var b in bytes)
                sb.Append(string.Format("%{0:X2}", b));

            return sb.ToString();
        }

        public static IDictionary<string, string> ParseQueryString(this string query)
        {
            // [DC]: This method does not URL decode, and cannot handle decoded input
            if (query.StartsWith("?"))
                query = query.Substring(1);

            if (query.Equals(string.Empty))
                return new Dictionary<string, string>();

            var parts = query.Split('&');

            return parts.Select(part => part.Split('='))
                .ToDictionary(pair => pair[0], pair => pair[1]);
        }
    }
}