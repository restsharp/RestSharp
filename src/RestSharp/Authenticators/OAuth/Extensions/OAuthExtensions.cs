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
using System.Security.Cryptography;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class OAuthExtensions
    {
        public static string ToRequestValue(this OAuthSignatureMethod signatureMethod)
        {
            var value    = signatureMethod.ToString().ToUpper();
            var shaIndex = value.IndexOf("SHA", StringComparison.Ordinal);

            return shaIndex > -1 ? value.Insert(shaIndex, "-") : value;
        }

        public static string HashWith(this string input, HashAlgorithm algorithm)
        {
            var data = Encoding.UTF8.GetBytes(input);
            var hash = algorithm.ComputeHash(data);

            return Convert.ToBase64String(hash);
        }
    }
}