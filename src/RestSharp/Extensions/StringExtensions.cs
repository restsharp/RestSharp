//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RestSharp.Extensions; 

static class StringExtensions {
    static readonly Regex IsUpperCaseRegex = new(@"^[A-Z]+$");

    static readonly Regex AddUnderscoresRegex1 = new(@"[-\s]");
    static readonly Regex AddUnderscoresRegex2 = new(@"([a-z\d])([A-Z])");
    static readonly Regex AddUnderscoresRegex3 = new(@"([A-Z]+)([A-Z][a-z])");

    static readonly Regex AddDashesRegex1 = new(@"[\s]");
    static readonly Regex AddDashesRegex2 = new(@"([a-z\d])([A-Z])");
    static readonly Regex AddDashesRegex3 = new(@"([A-Z]+)([A-Z][a-z])");

    static readonly Regex AddSpacesRegex1 = new(@"[-\s]");
    static readonly Regex AddSpacesRegex2 = new(@"([a-z\d])([A-Z])");
    static readonly Regex AddSpacesRegex3 = new(@"([A-Z]+)([A-Z][a-z])");
    public static string UrlDecode(this string input) => HttpUtility.UrlDecode(input);

    /// <summary>
    /// Uses Uri.EscapeDataString() based on recommendations on MSDN
    /// http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
    /// </summary>
    public static string UrlEncode(this string input) {
        const int maxLength = 32766;

        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (input.Length <= maxLength)
            return Uri.EscapeDataString(input);

        var sb    = new StringBuilder(input.Length * 2);
        var index = 0;

        while (index < input.Length) {
            var length = Math.Min(input.Length - index, maxLength);

            while (CharUnicodeInfo.GetUnicodeCategory(input[index + length - 1]) == UnicodeCategory.Surrogate) {
                length--;
            }

            var subString = input.Substring(index, length);

            sb.Append(Uri.EscapeDataString(subString));
            index += subString.Length;
        }

        return sb.ToString();
    }

    public static string? UrlEncode(this string input, Encoding encoding) {
        var encoded = HttpUtility.UrlEncode(input, encoding);
        return encoded?.Replace("+", "%20");
    }

    /// <summary>
    /// Remove underscores from a string
    /// </summary>
    /// <param name="input">String to process</param>
    /// <returns>string</returns>
    public static string RemoveUnderscoresAndDashes(this string input) => input.Replace("_", "").Replace("-", "");

    /// <summary>
    /// Converts a string to pascal case
    /// </summary>
    /// <param name="lowercaseAndUnderscoredWord">String to convert</param>
    /// <param name="culture"></param>
    /// <returns>string</returns>
    public static string ToPascalCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        => ToPascalCase(lowercaseAndUnderscoredWord, true, culture);

    /// <summary>
    /// Converts a string to pascal case with the option to remove underscores
    /// </summary>
    /// <param name="text">String to convert</param>
    /// <param name="removeUnderscores">Option to remove underscores</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string text, bool removeUnderscores, CultureInfo culture) {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace('_', ' ');

        var joinString = removeUnderscores ? string.Empty : "_";
        var words      = text.Split(' ');

        return words
            .Where(x => x.Length > 0)
            .Select(CaseWord)
            .JoinToString(joinString);

        string CaseWord(string word) {
            var restOfWord = word.Substring(1);
            var firstChar  = char.ToUpper(word[0], culture);

            if (restOfWord.IsUpperCase())
                restOfWord = restOfWord.ToLower(culture);

            return string.Concat(firstChar, restOfWord);
        }
    }

    /// <summary>
    /// Converts a string to camel case
    /// </summary>
    /// <param name="lowercaseAndUnderscoredWord">String to convert</param>
    /// <param name="culture"></param>
    /// <returns>String</returns>
    public static string ToCamelCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        => MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord, culture), culture);

    static string MakeInitialLowerCase(this string word, CultureInfo culture)
        => string.Concat(word.Substring(0, 1).ToLower(culture), word.Substring(1));

    static string AddUnderscores(this string pascalCasedWord)
        => AddUnderscoresRegex1.Replace(
            AddUnderscoresRegex2.Replace(
                AddUnderscoresRegex3.Replace(pascalCasedWord, "$1_$2"),
                "$1_$2"
            ),
            "_"
        );

    static string AddDashes(this string pascalCasedWord)
        => AddDashesRegex1.Replace(
            AddDashesRegex2.Replace(
                AddDashesRegex3.Replace(pascalCasedWord, "$1-$2"),
                "$1-$2"
            ),
            "-"
        );

    static bool IsUpperCase(this string inputString) => IsUpperCaseRegex.IsMatch(inputString);

    static string AddUnderscorePrefix(this string pascalCasedWord) => $"_{pascalCasedWord}";

    static string AddSpaces(this string pascalCasedWord)
        => AddSpacesRegex1.Replace(
            AddSpacesRegex2.Replace(
                AddSpacesRegex3.Replace(pascalCasedWord, "$1 $2"),
                "$1 $2"
            ),
            " "
        );

    internal static bool IsEmpty(this string? value) => string.IsNullOrWhiteSpace(value);

    internal static bool IsNotEmpty(this string? value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Return possible variants of a name for name matching.
    /// </summary>
    /// <param name="name">String to convert</param>
    /// <param name="culture">The culture to use for conversion</param>
    /// <returns>IEnumerable&lt;string&gt;</returns>
    public static IEnumerable<string> GetNameVariants(this string name, CultureInfo culture) {
        if (string.IsNullOrEmpty(name))
            yield break;

        yield return name;

        // try camel cased name
        yield return name.ToCamelCase(culture);

        // try lower cased name
        yield return name.ToLower(culture);

        // try name with underscores
        yield return name.AddUnderscores();

        // try name with underscores with lower case
        yield return name.AddUnderscores().ToLower(culture);

        // try name with dashes
        yield return name.AddDashes();

        // try name with dashes with lower case
        yield return name.AddDashes().ToLower(culture);

        // try name with underscore prefix
        yield return name.AddUnderscorePrefix();

        // try name with proper camel case
        yield return name.AddUnderscores().ToCamelCase(culture);

        // try name with underscore prefix, using proper camel case
        yield return name.ToCamelCase(culture).AddUnderscorePrefix();

        // try name with underscore prefix, using camel case
        yield return name.AddUnderscores().ToCamelCase(culture).AddUnderscorePrefix();

        // try name with spaces
        yield return name.AddSpaces();

        // try name with spaces with lower case
        yield return name.AddSpaces().ToLower(culture);
    }

    internal static string JoinToString<T>(this IEnumerable<T> collection, string separator, Func<T, string> getString)
        => JoinToString(collection.Select(getString), separator);

    internal static string JoinToString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);
}