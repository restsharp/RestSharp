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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace RestSharp.Extensions
{
	/// <summary>
	/// Extension method overload!
	/// </summary>
	public static class MiscExtensions
	{
		/// <summary>
		/// Check that a string is not null or empty
		/// </summary>
		/// <param name="input">String to check</param>
		/// <returns>bool</returns>
		public static bool HasValue(this string input) {
			return !string.IsNullOrEmpty(input);
		}

		/// <summary>
		/// Remove underscores from a string
		/// </summary>
		/// <param name="input">String to process</param>
		/// <returns>string</returns>
		public static string RemoveUnderscores(this string input) {
			return input.Replace("_", "");
		}

		/// <summary>
		/// Reads a stream into a string
		/// </summary>
		/// <param name="stream">Stream to read</param>
		/// <returns>string</returns>
		public static string ReadAsString(this Stream stream) {
			using (var reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Reads a byte array into a string using UTF8 encoding
		/// </summary>
		/// <param name="input">Bytes to read</param>
		/// <returns>string</returns>
		public static string ReadAsString(this byte[] input) {
			return Encoding.UTF8.GetString(input, 0, input.Length);
		}

		/// <summary>
		/// Save a byte array to a file
		/// </summary>
		/// <param name="input">Bytes to save</param>
		/// <param name="path">Full path to save file to</param>
		public static void SaveAs(this byte[] input, string path) {
			File.WriteAllBytes(path, input);
		}

		/// <summary>
		/// Read a stream into a byte array
		/// </summary>
		/// <param name="input">Stream to read</param>
		/// <returns>byte[]</returns>
		public static byte[] ReadAsBytes(this Stream input) {
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream()) {
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Parses most common JSON date formats
		/// </summary>
		/// <param name="input">JSON value to parse</param>
		/// <returns>DateTime</returns>
		public static DateTime ParseJsonDate(this string input) {
			input = input.Replace("\n", "");
			input = input.Replace("\r", "");

			input = input.RemoveSurroundingQuotes();

			if (input.Contains("/Date(")) {
				return ExtractDate(input, @"\\/Date\((-?\d+)(-|\+)?([0-9]{4})?\)\\/");
			}
			
			if (input.Contains("new Date(")) {
				input = input.Replace(" ", "");
				// because all whitespace is removed, match against newDate( instead of new Date(
				return ExtractDate(input, @"newDate\((-?\d+)*\)");
			}

			return ParseFormattedDate(input);
		}

		/// <summary>
		/// Remove leading and trailing " from a string
		/// </summary>
		/// <param name="input">String to parse</param>
		/// <returns>String</returns>
		public static string RemoveSurroundingQuotes(this string input) {
			if (input.StartsWith("\"") && input.EndsWith("\"")) {
				// remove leading/trailing quotes
				input = input.Substring(1, input.Length - 2); 
			}
			return input;
		}

		private static DateTime ParseFormattedDate(string input) {
			var formats = new[] {
				"u", 
				"s", 
				"yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", 
				"yyyy-MM-ddTHH:mm:ssZ", 
				"yyyy-MM-dd HH:mm:ssZ", 
				"yyyy-MM-ddTHH:mm:ss", 
				"yyyy-MM-ddTHH:mm:sszzzzzz"
			};

			DateTime date;
			if (DateTime.TryParseExact(input, formats,
									   CultureInfo.InvariantCulture,
									   DateTimeStyles.None, out date)) {
				return date;
			}

			return default(DateTime);
		}

		private static DateTime ExtractDate(string input, string pattern) {
			DateTime dt = DateTime.MinValue;
			var regex = new Regex(pattern);
			if (regex.IsMatch(input)) {
				var matches = regex.Matches(input);
				var match = matches[0];
				var ms = Convert.ToInt64(match.Groups[1].Value);
				var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				dt = epoch.AddMilliseconds(ms);

				// adjust if time zone modifier present
				if (match.Groups.Count > 2 && !String.IsNullOrEmpty(match.Groups[3].Value)) {
					var mod = DateTime.ParseExact(match.Groups[3].Value, "hhmm", CultureInfo.InvariantCulture);
					if (match.Groups[2].Value == "+") {
						dt = dt.Add(mod.TimeOfDay);
					}
					else {
						dt = dt.Subtract(mod.TimeOfDay);
					}
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
		public static bool Matches(this string input, string pattern) {
			return Regex.IsMatch(input, pattern);
		}

		/// <summary>
		/// Converts a string to pascal case
		/// </summary>
		/// <param name="lowercaseAndUnderscoredWord">String to convert</param>
		/// <returns>string</returns>
		public static string ToPascalCase(this string lowercaseAndUnderscoredWord) {
			return ToPascalCase(lowercaseAndUnderscoredWord, true);
		}

		/// <summary>
		/// Converts a string to pascal case with the option to remove underscores
		/// </summary>
		/// <param name="text">String to convert</param>
		/// <param name="removeUnderscores">Option to remove underscores</param>
		/// <returns></returns>
		public static string ToPascalCase(this string text, bool removeUnderscores) {
			if (String.IsNullOrEmpty(text))
				return text;

			text = text.Replace("_", " ");
			string joinString = removeUnderscores ? String.Empty : "_";
			string[] words = text.Split(' ');
			if (words.Length > 1 || words[0].IsUpperCase()) {
				for (int i = 0; i < words.Length; i++) {
					if (words[i].Length > 0) {
						string word = words[i];
						string restOfWord = word.Substring(1);

						if (restOfWord.IsUpperCase())
							restOfWord = restOfWord.ToLower(CultureInfo.CurrentUICulture);

						char firstChar = char.ToUpper(word[0], CultureInfo.CurrentUICulture);
						words[i] = String.Concat(firstChar, restOfWord);
					}
				}
				return String.Join(joinString, words);
			}
			return String.Concat(words[0].Substring(0, 1).ToUpper(CultureInfo.CurrentUICulture), words[0].Substring(1));
		}

		/// <summary>
		/// Converts a string to camel case
		/// </summary>
		/// <param name="lowercaseAndUnderscoredWord">String to convert</param>
		/// <returns>String</returns>
		public static string ToCamelCase(this string lowercaseAndUnderscoredWord) {
			return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
		}

		/// <summary>
		/// Convert the first letter of a string to lower case
		/// </summary>
		/// <param name="word">String to convert</param>
		/// <returns>string</returns>
		public static string MakeInitialLowerCase(this string word) {
			return String.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
		}

		/// <summary>
		/// Checks to see if a string is all uppper case
		/// </summary>
		/// <param name="inputString">String to check</param>
		/// <returns>bool</returns>
		public static bool IsUpperCase(this string inputString) {
			return Regex.IsMatch(inputString, @"^[A-Z]+$");
		}

		/// <summary>
		/// Add underscores to a pascal-cased string
		/// </summary>
		/// <param name="pascalCasedWord">String to convert</param>
		/// <returns>string</returns>
		public static string AddUnderscores(this string pascalCasedWord) {
			return
				Regex.Replace(
					Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
						"$1_$2"), @"[-\s]", "_").ToLower();
		}
	}
}