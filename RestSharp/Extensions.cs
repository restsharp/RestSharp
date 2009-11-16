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

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace RestSharp
{
	public static class Extensions
	{
		public static bool HasValue(this string input) {
			return !string.IsNullOrEmpty(input);
		}

		public static string RemoveUnderscores(this string input) {
			return input.Replace("_", "");
		}

		public static string ReadAsString(this Stream stream) {
			using (var reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}

		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic) {
			while (toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static DateTime ParseJsonDate(this string input) {
			if (input.Contains("/Date(")) {
				var regex = new Regex(@"\\/Date\((\d+)(-|\+)?([0-9]{4})?\)\\/");
				if (regex.IsMatch(input)) {
					var matches = regex.Matches(input);
					var match = matches[0];
					var ms = Convert.ToInt64(match.Groups[1].Value);
					var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					var dt = epoch.AddMilliseconds(ms);

					// adjust if time zone modifier present
					if (match.Groups[3] != null) {
						var mod = DateTime.ParseExact(match.Groups[3].Value, "hhmm", CultureInfo.InvariantCulture);
						if (match.Groups[2].Value == "+") {
							dt = dt.Add(mod.TimeOfDay); 
						}
						else {
							dt = dt.Subtract(mod.TimeOfDay);
						}
					}

					return dt;
				}
			}
			else if (input.Contains("new Date(")) {
				// TODO: implement parsing
			}
			else if (input.Matches(@"([0-9-])*T([0-9\:]*)Z")) {
				// TODO: implement parsing
			}

			return default(DateTime);
		}

		public static bool Matches(this string input, string pattern) {
			return Regex.IsMatch(input, pattern);
		}

		public static string ToPascalCase(this string lowercaseAndUnderscoredWord) {
			return ToPascalCase(lowercaseAndUnderscoredWord, true);
		}

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

		public static string ToCamelCase(this string lowercaseAndUnderscoredWord) {
			return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
		}

		public static string MakeInitialLowerCase(this string word) {
			return String.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
		}

		public static bool IsUpperCase(this string inputString) {
			return Regex.IsMatch(inputString, @"^[A-Z]+$");
		}

		public static string AddUnderscores(this string pascalCasedWord) {
			return
				Regex.Replace(
					Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
						"$1_$2"), @"[-\s]", "_").ToLower();
		}
	}
}