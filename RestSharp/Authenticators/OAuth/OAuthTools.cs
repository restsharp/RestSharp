using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using RestSharp.Authenticators.OAuth.Extensions;
using RestSharp.Extensions;

namespace RestSharp.Authenticators.OAuth
{
    [DataContract]
    internal static class OAuthTools
    {
        const string ALPHA_NUMERIC = UPPER + LOWER + DIGIT;

        const string DIGIT = "1234567890";

        const string LOWER = "abcdefghijklmnopqrstuvwxyz";

        const string UNRESERVED = ALPHA_NUMERIC + "-._~";

        const string UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static readonly Random random;

        static readonly object randomLock = new object();

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        /// <summary>
        ///     All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        static readonly Encoding encoding = Encoding.UTF8;

        /// <summary>
        ///     The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        static readonly string[] uriRfc3986CharsToEscape = {"!", "*", "'", "(", ")"};

        static readonly string[] uriRfc3968EscapedHex = {"%21", "%2A", "%27", "%28", "%29"};

        static OAuthTools()
        {
            var bytes = new byte[4];

            rng.GetBytes(bytes);
            random = new Random(BitConverter.ToInt32(bytes, 0));
        }

        /// <summary>
        ///     Generates a random 16-byte lowercase alphanumeric string.
        /// </summary>
        /// <returns></returns>
        public static string GetNonce()
        {
            const string chars = LOWER + DIGIT;

            var nonce = new char[16];

            lock (randomLock)
            {
                for (var i = 0; i < nonce.Length; i++)
                    nonce[i] = chars[random.Next(0, chars.Length)];
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
        public static string GetTimestamp(DateTime dateTime)
        {
            var timestamp = dateTime.ToUnixTime();

            return timestamp.ToString();
        }

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

            for (var i = 0; i < uriRfc3986CharsToEscape.Length; i++)
            {
                var t = uriRfc3986CharsToEscape[i];

                escapedRfc3986.Replace(t, uriRfc3968EscapedHex[i]);
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
        public static string UrlEncodeStrict(string value)
        {
            // From oauth spec above: -
            // Characters not in the unreserved character set ([RFC3986]
            // (Berners-Lee, T., "Uniform Resource Identifiers (URI):
            // Generic Syntax," .) section 2.3) MUST be encoded.
            // ...
            // unreserved = ALPHA, DIGIT, '-', '.', '_', '~'
            var result = "";

            value.ForEach(
                c =>
                {
                    result += UNRESERVED.Contains(c)
                        ? c.ToString()
                        : c.ToString()
                            .PercentEncode();
                }
            );

            return result;
        }

        /// <summary>
        ///     Sorts a collection of key-value pairs by name, and then value if equal,
        ///     concatenating them into a single string. This string should be encoded
        ///     prior to, or after normalization is run.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string NormalizeRequestParameters(WebParameterCollection parameters)
        {
            var copy         = SortParametersExcludingSignature(parameters);
            var concatenated = copy.Concatenate("=", "&");

            return concatenated;
        }

        /// <summary>
        ///     Sorts a <see cref="WebParameterCollection" /> by name, and then value if equal.
        /// </summary>
        /// <param name="parameters">A collection of parameters to sort</param>
        /// <returns>A sorted parameter collection</returns>
        public static WebParameterCollection SortParametersExcludingSignature(WebParameterCollection parameters)
        {
            var copy       = new WebParameterCollection(parameters);
            var exclusions = copy.Where(n => n.Name.EqualsIgnoreCase("oauth_signature"));

            copy.RemoveAll(exclusions);

            copy.ForEach(
                p =>
                {
                    p.Name  = UrlEncodeStrict(p.Name);
                    p.Value = UrlEncodeStrict(p.Value);
                }
            );

            copy.Sort(
                (x, y) =>
                {
                    var compareName = string.CompareOrdinal(x.Name, y.Name);

                    return compareName != 0
                        ? compareName
                        : string.CompareOrdinal(x.Value, y.Value);
                }
            );

            return copy;
        }

        /// <summary>
        ///     Creates a request URL suitable for making OAuth requests.
        ///     Resulting URLs must exclude port 80 or port 443 when accompanied by HTTP and HTTPS, respectively.
        ///     Resulting URLs must be lower case.
        /// </summary>
        /// <param name="url">The original request URL</param>
        /// <returns></returns>
        public static string ConstructRequestUrl(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var sb         = new StringBuilder();
            var requestUrl = "{0}://{1}".FormatWith(url.Scheme, url.Host);
            var qualified  = ":{0}".FormatWith(url.Port);
            var basic      = url.Scheme == "http"  && url.Port == 80;
            var secure     = url.Scheme == "https" && url.Port == 443;

            sb.Append(requestUrl);

            sb.Append(
                !basic && !secure
                    ? qualified
                    : ""
            );
            sb.Append(url.AbsolutePath);

            return sb.ToString(); //.ToLower();
        }

        /// <summary>
        ///     Creates a request elements concatenation value to send with a request.
        ///     This is also known as the signature base.
        /// </summary>
        /// <param name="method">The request HTTP method type</param>
        /// <param name="url">The request URL</param>
        /// <param name="parameters">The request parameters</param>
        /// <returns>A signature base string</returns>
        public static string ConcatenateRequestElements(string method, string url, WebParameterCollection parameters)
        {
            var sb = new StringBuilder();

            // Separating &'s are not URL encoded
            var requestMethod     = method.ToUpper().Then("&");
            var requestUrl        = UrlEncodeRelaxed(ConstructRequestUrl(url.AsUri())).Then("&");
            var requestParameters = UrlEncodeRelaxed(NormalizeRequestParameters(parameters));

            sb.Append(requestMethod);
            sb.Append(requestUrl);
            sb.Append(requestParameters);

            return sb.ToString();
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
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(
            OAuthSignatureMethod signatureMethod,
            string signatureBase,
            string consumerSecret,
            string tokenSecret
        )
            => GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, consumerSecret, tokenSecret);

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
            if (tokenSecret.IsNullOrBlank())
                tokenSecret = string.Empty;

            var unencodedConsumerSecret = consumerSecret;
            consumerSecret = Uri.EscapeDataString(consumerSecret);
            tokenSecret    = Uri.EscapeDataString(tokenSecret);

            string signature;

            switch (signatureMethod)
            {
                case OAuthSignatureMethod.HmacSha1:
                {
                    var crypto = new HMACSHA1();
                    var key    = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);

                    crypto.Key = encoding.GetBytes(key);
                    signature  = signatureBase.HashWith(crypto);
                    break;
                }

                case OAuthSignatureMethod.HmacSha256:
                {
                    var crypto = new HMACSHA256();
                    var key    = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);

                    crypto.Key = encoding.GetBytes(key);
                    signature  = signatureBase.HashWith(crypto);
                    break;
                }

                case OAuthSignatureMethod.RsaSha1:
                {
                    using (var provider = new RSACryptoServiceProvider {PersistKeyInCsp = false})
                    {
                        provider.FromXmlString2(unencodedConsumerSecret);

                        var hasher = new SHA1Managed();
                        var hash   = hasher.ComputeHash(encoding.GetBytes(signatureBase));

                        signature = Convert.ToBase64String(provider.SignHash(hash, CryptoConfig.MapNameToOID("SHA1")));
                    }

                    break;
                }

                case OAuthSignatureMethod.PlainText:
                {
                    signature = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);

                    break;
                }

                default:
                    throw new NotImplementedException("Only HMAC-SHA1, HMAC-SHA256, and RSA-SHA1 are currently supported.");
            }

            var result = signatureTreatment == OAuthSignatureTreatment.Escaped
                ? UrlEncodeRelaxed(signature)
                : signature;

            return result;
        }
    }
}