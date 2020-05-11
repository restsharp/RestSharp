//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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
using System.Linq;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string left, string right) => string.Equals(left, right, StringComparison.InvariantCultureIgnoreCase);

        public static string Then(this string input, string value) => string.Concat(input, value);

        public static Uri AsUri(this string value) => new Uri(value);

        public static byte[] GetBytes(this string input) => Encoding.UTF8.GetBytes(input);

        public static string PercentEncode(this string s) => string.Join("", s.GetBytes().Select(x => $"%{x:X2}"));
    }
}