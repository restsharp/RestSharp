using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE
	[Serializable]
#endif
	internal static class OAuthTools
	{
		private const string AlphaNumeric = Upper + Lower + Digit;
		private const string Digit = "1234567890";
		private const string Lower = "abcdefghijklmnopqrstuvwxyz";
		private const string Unreserved = AlphaNumeric + "-._~";
		private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private static readonly Random _random;
		private static readonly object _randomLock = new object();

#if !SILVERLIGHT && !WINDOWS_PHONE
		private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
#endif

		static OAuthTools()
		{
#if !SILVERLIGHT && !WINDOWS_PHONE
			var bytes = new byte[4];
			_rng.GetNonZeroBytes(bytes);
			_random = new Random(BitConverter.ToInt32(bytes, 0));
#else
			_random = new Random();
#endif
		}

		/// <summary>
		/// All text parameters are UTF-8 encoded (per section 5.1).
		/// </summary>
		/// <seealso cref="http://www.hueniverse.com/hueniverse/2008/10/beginners-gui-1.html"/> 
		private static readonly Encoding _encoding = Encoding.UTF8;

		/// <summary>
		/// Generates a random 16-byte lowercase alphanumeric string. 
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#nonce"/>
		/// <returns></returns>
		public static string GetNonce()
		{
			const string chars = (Lower + Digit);

			var nonce = new char[16];
			lock (_randomLock)
			{
				for (var i = 0; i < nonce.Length; i++)
				{
					nonce[i] = chars[_random.Next(0, chars.Length)];
				}
			}
			return new string(nonce);
		}

		/// <summary>
		/// Generates a timestamp based on the current elapsed seconds since '01/01/1970 0000 GMT"
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#nonce"/>
		/// <returns></returns>
		public static string GetTimestamp()
		{
			return GetTimestamp(DateTime.UtcNow);
		}

		/// <summary>
		/// Generates a timestamp based on the elapsed seconds of a given time since '01/01/1970 0000 GMT"
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#nonce"/>
		/// <param name="dateTime">A specified point in time.</param>
		/// <returns></returns>
		public static string GetTimestamp(DateTime dateTime)
		{
			var timestamp = dateTime.ToUnixTime();
			return timestamp.ToString();
		}

		/// <summary>
		/// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
		/// </summary>
		/// <seealso cref="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
		private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

		private static readonly string[] UriRfc3968EscapedHex = new[] {"%21", "%2A", "%27", "%28", "%29"};

		/// <summary>
		/// URL encodes a string based on section 5.1 of the OAuth spec.
		/// Namely, percent encoding with [RFC3986], avoiding unreserved characters,
		/// upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
		/// </summary>
		/// <param name="value">The value to escape.</param>
		/// <returns>The escaped value.</returns>
		/// <remarks>
		/// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
		/// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
		/// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
		/// host actually having this configuration element present.
		/// </remarks>
		/// <seealso cref="http://oauth.net/core/1.0#encoding_parameters" />
		/// <seealso cref="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
		public static string UrlEncodeRelaxed(string value)
		{
			// Start with RFC 2396 escaping by calling the .NET method to do the work.
			// This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
			// If it does, the escaping we do that follows it will be a no-op since the
			// characters we search for to replace can't possibly exist in the string.
			StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));

			// Upgrade the escaping to RFC 3986, if necessary.
			for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
			{
				string t = UriRfc3986CharsToEscape[i];
				escaped.Replace(t, UriRfc3968EscapedHex[i]);
			}

			// Return the fully-RFC3986-escaped string.
			return escaped.ToString();
		}

		/// <summary>
		/// URL encodes a string based on section 5.1 of the OAuth spec.
		/// Namely, percent encoding with [RFC3986], avoiding unreserved characters,
		/// upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
		/// </summary>
		/// <param name="value"></param>
		/// <seealso cref="http://oauth.net/core/1.0#encoding_parameters" />
		public static string UrlEncodeStrict(string value)
		{
			// [JD]: We need to escape the apostrophe as well or the signature will fail
			var original = value;
			var ret = original.Where(
				c => !Unreserved.Contains(c) && c != '%').Aggregate(
					value, (current, c) => current.Replace(
						c.ToString(), c.ToString().PercentEncode()
				));

			return ret.Replace("%%", "%25%"); // Revisit to encode actual %'s
		}

		/// <summary>
		/// Sorts a collection of key-value pairs by name, and then value if equal,
		/// concatenating them into a single string. This string should be encoded
		/// prior to, or after normalization is run.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.1.1"/>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static string NormalizeRequestParameters(WebParameterCollection parameters)
		{
			var copy = SortParametersExcludingSignature(parameters);
			var concatenated = copy.Concatenate("=", "&");
			return concatenated;
		}

		/// <summary>
		/// Sorts a <see cref="WebParameterCollection"/> by name, and then value if equal.
		/// </summary>
		/// <param name="parameters">A collection of parameters to sort</param>
		/// <returns>A sorted parameter collection</returns>
		public static WebParameterCollection SortParametersExcludingSignature(WebParameterCollection parameters)
		{
			var copy = new WebParameterCollection(parameters);
			var exclusions = copy.Where(n => n.Name.EqualsIgnoreCase("oauth_signature"));

			copy.RemoveAll(exclusions);
            copy.ForEach(p => { p.Name = UrlEncodeStrict(p.Name); p.Value = UrlEncodeStrict(p.Value); });
			copy.Sort(
				(x, y) =>
				string.CompareOrdinal(x.Name, y.Name) != 0
					? string.CompareOrdinal(x.Name, y.Name)
					: string.CompareOrdinal(x.Value, y.Value));
			return copy;
		}

		/// <summary>
		/// Creates a request URL suitable for making OAuth requests.
		/// Resulting URLs must exclude port 80 or port 443 when accompanied by HTTP and HTTPS, respectively.
		/// Resulting URLs must be lower case.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.1.2"/>
		/// <param name="url">The original request URL</param>
		/// <returns></returns>
		public static string ConstructRequestUrl(Uri url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url");
			}

			var sb = new StringBuilder();

			var requestUrl = "{0}://{1}".FormatWith(url.Scheme, url.Host);
			var qualified = ":{0}".FormatWith(url.Port);
			var basic = url.Scheme == "http" && url.Port == 80;
			var secure = url.Scheme == "https" && url.Port == 443;

			sb.Append(requestUrl);
			sb.Append(!basic && !secure ? qualified : "");
			sb.Append(url.AbsolutePath);

			return sb.ToString(); //.ToLower();
		}

		/// <summary>
		/// Creates a request elements concatentation value to send with a request. 
		/// This is also known as the signature base.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.1.3"/>
		/// <seealso cref="http://oauth.net/core/1.0#sig_base_example"/>
		/// <param name="method">The request's HTTP method type</param>
		/// <param name="url">The request URL</param>
		/// <param name="parameters">The request's parameters</param>
		/// <returns>A signature base string</returns>
		public static string ConcatenateRequestElements(string method, string url, WebParameterCollection parameters)
		{
			var sb = new StringBuilder();

			// Separating &'s are not URL encoded
			var requestMethod = method.ToUpper().Then("&");
			var requestUrl = UrlEncodeRelaxed(ConstructRequestUrl(url.AsUri())).Then("&");
			var requestParameters = UrlEncodeRelaxed(NormalizeRequestParameters(parameters));

			sb.Append(requestMethod);
			sb.Append(requestUrl);
			sb.Append(requestParameters);

			return sb.ToString();
		}

		/// <summary>
		/// Creates a signature value given a signature base and the consumer secret.
		/// This method is used when the token secret is currently unknown.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.2"/>
		/// <param name="signatureMethod">The hashing method</param>
		/// <param name="signatureBase">The signature base</param>
		/// <param name="consumerSecret">The consumer key</param>
		/// <returns></returns>
		public static string GetSignature(OAuthSignatureMethod signatureMethod, string signatureBase, string consumerSecret)
		{
			return GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, signatureBase, consumerSecret, null);
		}

		/// <summary>
		/// Creates a signature value given a signature base and the consumer secret.
		/// This method is used when the token secret is currently unknown.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.2"/>
		/// <param name="signatureMethod">The hashing method</param>
		/// <param name="signatureTreatment">The treatment to use on a signature value</param>
		/// <param name="signatureBase">The signature base</param>
		/// <param name="consumerSecret">The consumer key</param>
		/// <returns></returns>
		public static string GetSignature(OAuthSignatureMethod signatureMethod, OAuthSignatureTreatment signatureTreatment, string signatureBase, string consumerSecret)
		{
			return GetSignature(signatureMethod, signatureTreatment, signatureBase, consumerSecret, null);
		}

		/// <summary>
		/// Creates a signature value given a signature base and the consumer secret and a known token secret.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.2"/>
		/// <param name="signatureMethod">The hashing method</param>
		/// <param name="signatureBase">The signature base</param>
		/// <param name="consumerSecret">The consumer secret</param>
		/// <param name="tokenSecret">The token secret</param>
		/// <returns></returns>
		public static string GetSignature(OAuthSignatureMethod signatureMethod, string signatureBase, string consumerSecret, string tokenSecret)
		{
			return GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, consumerSecret, tokenSecret);
		}

		/// <summary>
		/// Creates a signature value given a signature base and the consumer secret and a known token secret.
		/// </summary>
		/// <seealso cref="http://oauth.net/core/1.0#rfc.section.9.2"/>
		/// <param name="signatureMethod">The hashing method</param>
		/// <param name="signatureTreatment">The treatment to use on a signature value</param>
		/// <param name="signatureBase">The signature base</param>
		/// <param name="consumerSecret">The consumer secret</param>
		/// <param name="tokenSecret">The token secret</param>
		/// <returns></returns>
		public static string GetSignature(OAuthSignatureMethod signatureMethod,
			OAuthSignatureTreatment signatureTreatment,
			string signatureBase,
			string consumerSecret,
			string tokenSecret)
		{
			if (tokenSecret.IsNullOrBlank())
			{
				tokenSecret = String.Empty;
			}

			consumerSecret = UrlEncodeRelaxed(consumerSecret);
			tokenSecret = UrlEncodeRelaxed(tokenSecret);

			string signature;
			switch (signatureMethod)
			{
				case OAuthSignatureMethod.HmacSha1:
				{
					var crypto = new HMACSHA1();
					var key = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);

					crypto.Key = _encoding.GetBytes(key);
					signature = signatureBase.HashWith(crypto);

					break;
				}
				case OAuthSignatureMethod.PlainText:
				{
				    signature = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);

				    break;
				}
				default:
					throw new NotImplementedException("Only HMAC-SHA1 is currently supported.");
			}

			var result = signatureTreatment == OAuthSignatureTreatment.Escaped
				? UrlEncodeRelaxed(signature)
				: signature;

			return result;
		}
	}
}