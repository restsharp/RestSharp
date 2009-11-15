using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
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

			}
			else if (input.Matches(@"([0-9-])*T([0-9\:]*)Z")) {

			}

			return default(DateTime);
		}

		public static bool Matches(this string input, string pattern) {
			return Regex.IsMatch(input, pattern);
		}
	}
}