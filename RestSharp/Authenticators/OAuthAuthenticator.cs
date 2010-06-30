﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RestSharp.Authenticators
{
    public class OAuthAuthenticator : IAuthenticator
    {
        private readonly string _baseUrl;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _token;
        private readonly string _tokenSecret;

        private static readonly Random Random = new Random();
        private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private const string ConsumerKeyKey = "oauth_consumer_key";
        private const string TokenKey = "oauth_token";
        private const string NonceKey = "oauth_nonce";
        private const string TimestampKey = "oauth_timestamp";
        private const string SignatureMethodKey = "oauth_signature_method";
        private const string SignatureMethod = "HMAC-SHA1";
        private const string SignatureKey = "oauth_signature";
        private const string VersionKey = "oauth_version";
        private const string Version = "1.0";

        public OAuthAuthenticator(string baseUrl, string consumerKey, string consumerSecret)
            : this(baseUrl, consumerKey, consumerSecret, string.Empty, string.Empty)
        { }

        public OAuthAuthenticator(string baseUrl, string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _baseUrl = baseUrl;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        public void Authenticate(RestRequest request)
        {
            request.AddParameter(VersionKey, Version);
            request.AddParameter(NonceKey, GenerateNonce());
            request.AddParameter(TimestampKey, GenerateTimeStamp());
            request.AddParameter(SignatureMethodKey, SignatureMethod);
            request.AddParameter(ConsumerKeyKey, _consumerKey);

            if (!string.IsNullOrEmpty(_token))
                request.AddParameter(TokenKey, _token);

            // sort all the parameters
            request.Parameters.Sort(new QueryParameterComparer());

            request.AddParameter(SignatureKey, GenerateSignature(request));
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return Random.Next(123400, 9999999).ToString();
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        protected string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        private string GenerateSignature(RestRequest request)
        {
            var url = BuildUri(request);

            string normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);

            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
                normalizedUrl += ":" + url.Port;

            normalizedUrl += url.AbsolutePath;
            string normalizedRequestParameters = NormalizeRequestParameters(request.Parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", request.Method.ToString().ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            var signature = signatureBase.ToString();

            var hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(_consumerSecret), string.IsNullOrEmpty(_tokenSecret) ? string.Empty : UrlEncode(_tokenSecret)));

            return ComputeHash(hmacsha1, signature);
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        private static string NormalizeRequestParameters(IList<Parameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            Parameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                    sb.Append("&");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algorithm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        private Uri BuildUri(RestRequest request)
        {
            var assembled = request.Resource;
            var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);

            assembled = urlParms.Aggregate(assembled, (current, p) => current.Replace("{" + p.Name + "}", p.Value.ToString()));

            return new Uri(string.Format("{0}/{1}", _baseUrl, assembled));
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        private class QueryParameterComparer : IComparer<Parameter>
        {
            public int Compare(Parameter x, Parameter y)
            {
                return x.Name == y.Name ? string.Compare(x.Value.ToString(), y.Value.ToString())
                                        : string.Compare(x.Name, y.Name);
            }
        }

    }
}
