using System;
using System.Linq;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrBlank(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool EqualsIgnoreCase(this string left, string right) => string.Equals(left, right, StringComparison.InvariantCultureIgnoreCase);

        public static string Then(this string input, string value) => string.Concat(input, value);

        public static Uri AsUri(this string value) => new Uri(value);

        public static byte[] GetBytes(this string input) => Encoding.UTF8.GetBytes(input);

        public static string PercentEncode(this string s) => string.Join("", s.GetBytes().Select(x => $"%{x:X2}"));
    }
}