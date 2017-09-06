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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

#if WINDOWS_PHONE
using System.Net;
#endif

#if SILVERLIGHT
using System.Windows.Browser;
#endif

#if !SILVERLIGHT && !WINDOWS_PHONE
using RestSharp.Extensions.MonoHttp;
#endif

namespace RestSharp.Extensions
{
    public static class StringExtensions
    {
        public static string UrlDecode(this string input)
        {
            return HttpUtility.UrlDecode(input);
        }

        /// <summary>
        /// Uses Uri.EscapeDataString() based on recommendations on MSDN
        /// http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
        /// </summary>
        public static string UrlEncode(this string input)
        {
            const int maxLength = 32766;

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length <= maxLength)
            {
                return Uri.EscapeDataString(input);
            }

            StringBuilder sb = new StringBuilder(input.Length * 2);
            int index = 0;

            while (index < input.Length)
            {
                int length = Math.Min(input.Length - index, maxLength);
                string subString = input.Substring(index, length);

                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }

        public static string HtmlDecode(this string input)
        {
            return HttpUtility.HtmlDecode(input);
        }

        public static string HtmlEncode(this string input)
        {
            return HttpUtility.HtmlEncode(input);
        }

#if FRAMEWORK
        public static string HtmlAttributeEncode(this string input)
        {
            return HttpUtility.HtmlAttributeEncode(input);
        }
#endif

        /// <summary>
        /// Check that a string is not null or empty
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Remove underscores from a string
        /// </summary>
        /// <param name="input">String to process</param>
        /// <returns>string</returns>
        public static string RemoveUnderscoresAndDashes(this string input)
        {
            return input.Replace("_", "")
                        .Replace("-", ""); // avoiding regex
        }

        /// <summary>
        /// Parses most common JSON date formats
        /// </summary>
        /// <param name="input">JSON value to parse</param>
        /// <param name="culture"></param>
        /// <returns>DateTime</returns>
        public static DateTime ParseJsonDate(this string input, CultureInfo culture)
        {
            const long maxAllowedTimestamp = 253402300799;
            
            input = input.Replace("\n", "");
            input = input.Replace("\r", "");
            input = input.RemoveSurroundingQuotes();

            long unix;

            if (long.TryParse(input, out unix))
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                if (unix > maxAllowedTimestamp)
                    return epoch.AddMilliseconds(unix);
                else
                    return epoch.AddSeconds(unix);
            }

            if (input.Contains("/Date("))
            {
                return ExtractDate(input, @"\\?/Date\((-?\d+)(-|\+)?([0-9]{4})?\)\\?/", culture);
            }

            if (input.Contains("new Date("))
            {
                input = input.Replace(" ", "");

                // because all whitespace is removed, match against newDate( instead of new Date(
                return ExtractDate(input, @"newDate\((-?\d+)*\)", culture);
            }

            return ParseFormattedDate(input, culture);
        }

        /// <summary>
        /// Remove leading and trailing " from a string
        /// </summary>
        /// <param name="input">String to parse</param>
        /// <returns>String</returns>
        public static string RemoveSurroundingQuotes(this string input)
        {
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                // remove leading/trailing quotes
                input = input.Substring(1, input.Length - 2);
            }

            return input;
        }

        private static DateTime ParseFormattedDate(string input, CultureInfo culture)
        {
            string[] formats =
            {
                "u",
                "s",
                "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-dd HH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:sszzzzzz",
                "M/d/yyyy h:mm:ss tt" // default format for invariant culture
            };

            DateTime date;

            if (DateTime.TryParseExact(input, formats, culture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out date))
            {
                return date;
            }

            if (DateTime.TryParse(input, culture, DateTimeStyles.None, out date))
            {
                return date;
            }

            return default(DateTime);
        }

        private static DateTime ExtractDate(string input, string pattern, CultureInfo culture)
        {
            DateTime dt = DateTime.MinValue;
            Regex regex = new Regex(pattern);

            if (regex.IsMatch(input))
            {
                MatchCollection matches = regex.Matches(input);
                Match match = matches[0];
                long ms = Convert.ToInt64(match.Groups[1].Value);
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                dt = epoch.AddMilliseconds(ms);

                // adjust if time zone modifier present
                if (match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    DateTime mod = DateTime.ParseExact(match.Groups[3].Value, "HHmm", culture);

                    dt = match.Groups[2].Value == "+"
                        ? dt.Add(mod.TimeOfDay)
                        : dt.Subtract(mod.TimeOfDay);
                }
            }

            return dt;
        }

        /// <summary>
        /// Checks a string to see if it matches a regex
        /// </summary>
        /// <param name="input">String to check</param>
        /// <param name="pattern">Pattern to match</param>
        /// <returns>bool</returns>
        public static bool Matches(this string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// Converts a string to pascal case
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">String to convert</param>
        /// <param name="culture"></param>
        /// <returns>string</returns>
        public static string ToPascalCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        {
            return ToPascalCase(lowercaseAndUnderscoredWord, true, culture);
        }

        /// <summary>
        /// Converts a string to pascal case with the option to remove underscores
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <param name="removeUnderscores">Option to remove underscores</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string text, bool removeUnderscores, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = text.Replace("_", " ");

            string joinString = removeUnderscores
                ? string.Empty
                : "_";
            string[] words = text.Split(' ');

            if (words.Length > 1 || words[0].IsUpperCase())
            {
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Length > 0)
                    {
                        string word = words[i];
                        string restOfWord = word.Substring(1);

                        if (restOfWord.IsUpperCase())
                        {
#if !WINDOWS_UWP
                            restOfWord = restOfWord.ToLower(culture);
#else
                            restOfWord = restOfWord.ToLower();
#endif
                        }

#if !WINDOWS_UWP                        
                        char firstChar = char.ToUpper(word[0], culture);
#else
                        char firstChar = char.ToUpper(word[0]);
#endif

                        words[i] = string.Concat(firstChar, restOfWord);
                    }
                }

                return string.Join(joinString, words);
            }

#if !WINDOWS_UWP
            return string.Concat(words[0].Substring(0, 1).ToUpper(culture), words[0].Substring(1));
#else
            return string.Concat(words[0].Substring(0, 1).ToUpper(), words[0].Substring(1));
#endif
        }

        /// <summary>
        /// Converts a string to camel case
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">String to convert</param>
        /// <param name="culture"></param>
        /// <returns>String</returns>
        public static string ToCamelCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        {
            return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord, culture));
        }

        /// <summary>
        /// Convert the first letter of a string to lower case
        /// </summary>
        /// <param name="word">String to convert</param>
        /// <returns>string</returns>
        public static string MakeInitialLowerCase(this string word)
        {
            return string.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
        }

        /// <summary>
        /// Checks to see if a string is all uppper case
        /// </summary>
        /// <param name="inputString">String to check</param>
        /// <returns>bool</returns>
        public static bool IsUpperCase(this string inputString)
        {
            return Regex.IsMatch(inputString, @"^[A-Z]+$");
        }

        /// <summary>
        /// Add underscores to a pascal-cased string
        /// </summary>
        /// <param name="pascalCasedWord">String to convert</param>
        /// <returns>string</returns>
        public static string AddUnderscores(this string pascalCasedWord)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"),
                    @"([a-z\d])([A-Z])",
                    "$1_$2"),
                @"[-\s]",
                "_");
        }

        /// <summary>
        /// Add dashes to a pascal-cased string
        /// </summary>
        /// <param name="pascalCasedWord">String to convert</param>
        /// <returns>string</returns>
        public static string AddDashes(this string pascalCasedWord)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1-$2"),
                    @"([a-z\d])([A-Z])",
                    "$1-$2"),
                @"[\s]",
                "-");
        }

        /// <summary>
        /// Add an undescore prefix to a pascasl-cased string
        /// </summary>
        /// <param name="pascalCasedWord"></param>
        /// <returns></returns>
        public static string AddUnderscorePrefix(this string pascalCasedWord)
        {
            return string.Format("_{0}", pascalCasedWord);
        }

        /// <summary>
        /// Add spaces to a pascal-cased string
        /// </summary>
        /// <param name="pascalCasedWord">String to convert</param>
        /// <returns>string</returns>
        public static string AddSpaces(this string pascalCasedWord)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1 $2"),
                    @"([a-z\d])([A-Z])",
                    "$1 $2"),
                @"[-\s]",
                " ");
        }

        /// <summary>
        /// Return possible variants of a name for name matching.
        /// </summary>
        /// <param name="name">String to convert</param>
        /// <param name="culture">The culture to use for conversion</param>
        /// <returns>IEnumerable&lt;string&gt;</returns>
        public static IEnumerable<string> GetNameVariants(this string name, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(name))
            {
                yield break;
            }

            yield return name;

            // try camel cased name
            yield return name.ToCamelCase(culture);

            // try lower cased name
#if !WINDOWS_UWP
            yield return name.ToLower(culture);
#else
            yield return name.ToLowerInvariant();
#endif

            // try name with underscores
            yield return name.AddUnderscores();

            // try name with underscores with lower case
#if !WINDOWS_UWP
            yield return name.AddUnderscores().ToLower(culture);
#else
            yield return name.AddUnderscores().ToLowerInvariant();
#endif

            // try name with dashes
            yield return name.AddDashes();

            // try name with dashes with lower case
#if !WINDOWS_UWP
            yield return name.AddDashes().ToLower(culture);
#else
            yield return name.AddDashes().ToLowerInvariant();
#endif

            // try name with underscore prefix
            yield return name.AddUnderscorePrefix();

            // try name with underscore prefix, using camel case
            yield return name.ToCamelCase(culture).AddUnderscorePrefix();

            // try name with spaces
            yield return name.AddSpaces();

            // try name with spaces with lower case
#if !WINDOWS_UWP
            yield return name.AddSpaces().ToLower(culture);
#else
            yield return name.AddSpaces().ToLowerInvariant();
#endif
        }
    }
}
