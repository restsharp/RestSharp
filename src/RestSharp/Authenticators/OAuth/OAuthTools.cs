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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;
using RestSharp.Validation;
using static RestSharp.Authenticators.OAuth.OAuthSignatureMethod;

namespace RestSharp.Authenticators.OAuth
{
    internal static class OAuthTools
    {
        const string AlphaNumeric = Upper + Lower + Digit;

        const string Digit = "1234567890";

        const string Lower = "abcdefghijklmnopqrstuvwxyz";

        const string Unreserved = AlphaNumeric + "-._~";

        const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static readonly Random Random;

        static readonly object RandomLock = new object();

        static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        /// <summary>
        ///     All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        static readonly Encoding Encoding = Encoding.UTF8;

        /// <summary>
        ///     The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        static readonly string[] UriRfc3986CharsToEscape = {"!", "*", "'", "(", ")"};

        static readonly string[] UriRfc3968EscapedHex = {"%21", "%2A", "%27", "%28", "%29"};

        static OAuthTools()
        {
            var bytes = new byte[4];

            Rng.GetBytes(bytes);
            Random = new Random(BitConverter.ToInt32(bytes, 0));
        }

        /// <summary>
        ///     Generates a random 16-byte lowercase alphanumeric string.
        /// </summary>
        /// <returns></returns>
        public static string GetNonce()
        {
            const string chars = Lower + Digit;

            var nonce = new char[16];

            lock (RandomLock)
            {
                for (var i = 0; i < nonce.Length; i++)
                    nonce[i] = chars[Random.Next(0, chars.Length)];
            }

            return new string(nonce);
        }

        /// <summary>
        ///     Generates a timestamp based on the current elapsed seconds since '01/01/1970 0000 GMT"
        /// </summary>
        /// <returns></returns>
        public static string GetTimestamp() => GetTimestamp(DateTime.UtcNow);

        /// <summary>
        ///     Generates a timestamp based on the elapsed seconds of a given time since '01/01/1970 0000 GMT"
        /// </summary>
        /// <param name="dateTime">A specified point in time.</param>
        /// <returns></returns>
        static string GetTimestamp(DateTime dateTime) => dateTime.ToUnixTime().ToString();

        /// <summary>
        ///     URL encodes a string based on section 5.1 of the OAuth spec.
        ///     Namely, percent encoding with [RFC3986], avoiding unreserved characters,
        ///     upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        ///     The <see cref="Uri.EscapeDataString" /> method is <i>supposed</i> to take on
        ///     RFC 3986 behavior if certain elements are present in a .config file.  Even if this
        ///     actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        ///     host actually having this configuration element present.
        /// </remarks>
        public static string UrlEncodeRelaxed(string value)
        {
            // Escape RFC 3986 chars first.
            var escapedRfc3986 = new StringBuilder(value);

            for (var i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                var t = UriRfc3986CharsToEscape[i];

                escapedRfc3986.Replace(t, UriRfc3968EscapedHex[i]);
            }

            // Do RFC 2396 escaping by calling the .NET method to do the work.
            var escapedRfc2396 = Uri.EscapeDataString(escapedRfc3986.ToString());

            // Return the fully-RFC3986-escaped string.
            return escapedRfc2396;
        }

        /// <summary>
        ///     URL encodes a string based on section 5.1 of the OAuth spec.
        ///     Namely, percent encoding with [RFC3986], avoiding unreserved characters,
        ///     upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
        /// </summary>
        /// <param name="value"></param>
        // From oauth spec above: -
        // Characters not in the unreserved character set ([RFC3986]
        // (Berners-Lee, T., "Uniform Resource Identifiers (URI):
        // Generic Syntax," .) section 2.3) MUST be encoded.
        // ...
        // unreserved = ALPHA, DIGIT, '-', '.', '_', '~'
        public static string UrlEncodeStrict(string value)
            => string.Join("", value.Select(x => Unreserved.Contains(x) ? x.ToString() : $"%{(byte) x:X2}"));

        /// <summary>
        ///     Sorts a collection of key-value pairs by name, and then value if equal,
        ///     concatenating them into a single string. This string should be encoded
        ///     prior to, or after normalization is run.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static string NormalizeRequestParameters(WebPairCollection parameters)
            => string.Join("&", SortParametersExcludingSignature(parameters));

        /// <summary>
        ///     Sorts a <see cref="WebPairCollection" /> by name, and then value if equal.
        /// </summary>
        /// <param name="parameters">A collection of parameters to sort</param>
        /// <returns>A sorted parameter collection</returns>
        public static IEnumerable<string> SortParametersExcludingSignature(WebPairCollection parameters)
            => parameters
                .Where(x => !x.Name.EqualsIgnoreCase("oauth_signature"))
                .Select(x => new WebPair(UrlEncodeStrict(x.Name), UrlEncodeStrict(x.Value)))
                .OrderBy(x => x, WebPair.Comparer)
                .Select(x => $"{x.Name}={x.Value}");

        /// <summary>
        ///     Creates a request URL suitable for making OAuth requests.
        ///     Resulting URLs must exclude port 80 or port 443 when accompanied by HTTP and HTTPS, respectively.
        ///     Resulting URLs must be lower case.
        /// </summary>
        /// <param name="url">The original request URL</param>
        /// <returns></returns>
        static string ConstructRequestUrl(Uri url)
        {
            Ensure.NotNull(url, nameof(url));

            var basic  = url.Scheme == "http"  && url.Port == 80;
            var secure = url.Scheme == "https" && url.Port == 443;
            var port   = basic || secure ? "" : $":{url.Port}";

            return $"{url.Scheme}://{url.Host}{port}{url.AbsolutePath}";
        }

        /// <summary>
        ///     Creates a request elements concatenation value to send with a request.
        ///     This is also known as the signature base.
        /// </summary>
        /// <param name="method">The request HTTP method type</param>
        /// <param name="url">The request URL</param>
        /// <param name="parameters">The request parameters</param>
        /// <returns>A signature base string</returns>
        public static string ConcatenateRequestElements(string method, string url, WebPairCollection parameters)
        {
            // Separating &'s are not URL encoded
            var requestMethod     = method.ToUpper().Then("&");
            var requestUrl        = UrlEncodeRelaxed(ConstructRequestUrl(url.AsUri())).Then("&");
            var requestParameters = UrlEncodeRelaxed(NormalizeRequestParameters(parameters));

            return $"{requestMethod}{requestUrl}{requestParameters}";
        }

        /// <summary>
        ///     Creates a signature value given a signature base and the consumer secret.
        ///     This method is used when the token secret is currently unknown.
        /// </summary>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(
            OAuthSignatureMethod signatureMethod,
            string signatureBase,
            string consumerSecret
        )
            => GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, signatureBase, consumerSecret, null);

        /// <summary>
        ///     Creates a signature value given a signature base and the consumer secret.
        ///     This method is used when the token secret is currently unknown.
        /// </summary>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(
            OAuthSignatureMethod signatureMethod,
            OAuthSignatureTreatment signatureTreatment,
            string signatureBase,
            string consumerSecret
        )
            => GetSignature(signatureMethod, signatureTreatment, signatureBase, consumerSecret, null);

        /// <summary>
        ///     Creates a signature value given a signature base and the consumer secret and a known token secret.
        /// </summary>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(
            OAuthSignatureMethod signatureMethod,
            OAuthSignatureTreatment signatureTreatment,
            string signatureBase,
            string consumerSecret,
            string tokenSecret
        )
        {
            if (tokenSecret.IsEmpty())
                tokenSecret = string.Empty;

            var unencodedConsumerSecret = consumerSecret;
            consumerSecret = Uri.EscapeDataString(consumerSecret);
            tokenSecret    = Uri.EscapeDataString(tokenSecret);

            var signature = signatureMethod switch
            {
                HmacSha1   => GetHmacSignature(new HMACSHA1(), consumerSecret, tokenSecret, signatureBase),
                HmacSha256 => GetHmacSignature(new HMACSHA256(), consumerSecret, tokenSecret, signatureBase),
                RsaSha1    => GetRsaSignature(),
                PlainText  => $"{consumerSecret}&{tokenSecret}",
                _          => throw new NotImplementedException("Only HMAC-SHA1, HMAC-SHA256, and RSA-SHA1 are currently supported.")
            };

            var result = signatureTreatment == OAuthSignatureTreatment.Escaped
                ? UrlEncodeRelaxed(signature)
                : signature;

            return result;

            string GetRsaSignature()
            {
                using var provider = new RSACryptoServiceProvider {PersistKeyInCsp = false};

                provider.FromXmlString2(unencodedConsumerSecret);

                var hasher = new SHA1Managed();
                var hash   = hasher.ComputeHash(Encoding.GetBytes(signatureBase));

                return Convert.ToBase64String(provider.SignHash(hash, CryptoConfig.MapNameToOID("SHA1")));
            }
        }

        static string GetHmacSignature(KeyedHashAlgorithm crypto, string consumerSecret, string tokenSecret, string signatureBase)
        {
            var key = $"{consumerSecret}&{tokenSecret}";
            crypto.Key = Encoding.GetBytes(key);
            return signatureBase.HashWith(crypto);
        }
    }
}