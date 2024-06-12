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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RestSharp.Extensions;

// ReSharper disable once PartialTypeWithSinglePart
static partial class StringExtensions {
    static readonly Regex IsUpperCaseRegex = IsUpperCase();

    static readonly Regex AddUnderscoresRegex1 = AddUnderscores1();
    static readonly Regex AddUnderscoresRegex2 = AddUnderscores2();
    static readonly Regex AddUnderscoresRegex3 = AddUnderscores3();

    static readonly Regex AddDashesRegex1 = AddDashes1();
    static readonly Regex AddDashesRegex2 = AddDashes2();
    static readonly Regex AddDashesRegex3 = AddDashes3();

    static readonly Regex AddSpacesRegex1 = AddSpaces1();
    static readonly Regex AddSpacesRegex2 = AddSpaces2();
    static readonly Regex AddSpacesRegex3 = AddSpaces3();

    internal static string UrlDecode(this string input) => HttpUtility.UrlDecode(input);

    /// <summary>
    /// Uses Uri.EscapeDataString() based on recommendations on MSDN
    /// http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
    /// </summary>
    internal static string UrlEncode(this string input) {
        const int maxLength = 32766;

        if (input == null) throw new ArgumentNullException(nameof(input));

        if (input.Length <= maxLength) return Uri.EscapeDataString(input);

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

    internal static string? UrlEncode(this string? input, Encoding encoding) {
        var encoded = HttpUtility.UrlEncode(input, encoding);
        return encoded?.Replace("+", "%20");
    }

    internal static string RemoveUnderscoresAndDashes(this string input) => input.Replace("_", "").Replace("-", "");

    internal static string ToPascalCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        => ToPascalCase(lowercaseAndUnderscoredWord, true, culture);

    internal static string ToPascalCase(this string text, bool removeUnderscores, CultureInfo culture) {
        if (string.IsNullOrEmpty(text)) return text;

        text = text.Replace('_', ' ');

        var joinString = removeUnderscores ? string.Empty : "_";
        var words      = text.Split(' ');

        return words
            .Where(x => x.Length > 0)
            .Select(CaseWord)
            .JoinToString(joinString);

        string CaseWord(string word) {
            var restOfWord = word[1..];
            var firstChar  = char.ToUpper(word[0], culture);

            if (restOfWord.IsUpperCase()) restOfWord = restOfWord.ToLower(culture);

            return string.Concat(firstChar, restOfWord);
        }
    }

    internal static string ToCamelCase(this string lowercaseAndUnderscoredWord, CultureInfo culture)
        => MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord, culture), culture);

    internal static IEnumerable<string> GetNameVariants(this string name, CultureInfo culture) {
        if (string.IsNullOrEmpty(name)) yield break;

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

    internal static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    internal static bool IsNotEmpty([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

    internal static string JoinToString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

    static string MakeInitialLowerCase(this string word, CultureInfo culture) => string.Concat(word[..1].ToLower(culture), word[1..]);

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

    const string RIsUpperCase    = "^[A-Z]+$";
    const string RAddUnderscore1 = @"[-\s]";
    const string RAddUnderscore2 = @"([a-z\d])([A-Z])";
    const string RAddUnderscore3 = "([A-Z]+)([A-Z][a-z])";
    const string RAddDashes1     = @"[\s]";
    const string RAddDashes2     = @"([a-z\d])([A-Z])";
    const string RAddDashes3     = "([A-Z]+)([A-Z][a-z])";
    const string RAddSpaces1     = @"[-\s]";
    const string RAddSpaces2     = @"([a-z\d])([A-Z])";
    const string RAddSpaces3     = "([A-Z]+)([A-Z][a-z])";

#if NET7_0_OR_GREATER
    [GeneratedRegex(RIsUpperCase)]
    private static partial Regex IsUpperCase();

    [GeneratedRegex(RAddUnderscore1)]
    private static partial Regex AddUnderscores1();

    [GeneratedRegex(RAddUnderscore2)]
    private static partial Regex AddUnderscores2();

    [GeneratedRegex(RAddUnderscore3)]
    private static partial Regex AddUnderscores3();

    [GeneratedRegex(RAddDashes1)]
    private static partial Regex AddDashes1();

    [GeneratedRegex(RAddDashes2)]
    private static partial Regex AddDashes2();

    [GeneratedRegex(RAddDashes3)]
    private static partial Regex AddDashes3();

    [GeneratedRegex(RAddSpaces1)]
    private static partial Regex AddSpaces1();

    [GeneratedRegex(RAddSpaces2)]
    private static partial Regex AddSpaces2();

    [GeneratedRegex(RAddSpaces3)]
    private static partial Regex AddSpaces3();
#else
    static Regex IsUpperCase() => new(RIsUpperCase);

    static Regex AddUnderscores1() => new(RAddUnderscore1);

    static Regex AddUnderscores2() => new(RAddUnderscore2);

    static Regex AddUnderscores3() => new(RAddUnderscore3);

    static Regex AddDashes1() => new(RAddDashes1);

    static Regex AddDashes2() => new(RAddDashes2);

    static Regex AddDashes3() => new(RAddDashes3);

    static Regex AddSpaces1() => new(RAddSpaces1);

    static Regex AddSpaces2() => new(RAddSpaces1);

    static Regex AddSpaces3() => new(RAddSpaces1);
#endif
}