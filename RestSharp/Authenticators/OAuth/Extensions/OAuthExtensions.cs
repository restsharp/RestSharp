using System;
using System.Security.Cryptography;
using System.Text;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class OAuthExtensions
    {
        public static string ToRequestValue(this OAuthSignatureMethod signatureMethod)
        {
            string value = signatureMethod.ToString()
                                          .ToUpper();
            int shaIndex = value.IndexOf("SHA");

            return shaIndex > -1
                ? value.Insert(shaIndex, "-")
                : value;
        }

        public static OAuthSignatureMethod FromRequestValue(this string signatureMethod)
        {
            switch (signatureMethod)
            {
                case "HMAC-SHA1":
                    return OAuthSignatureMethod.HmacSha1;

                case "HMAC-SHA256":
                    return OAuthSignatureMethod.HmacSha256;

                case "RSA-SHA1":
                    return OAuthSignatureMethod.RsaSha1;

                default:
                    return OAuthSignatureMethod.PlainText;
            }
        }

        public static string HashWith(this string input, HashAlgorithm algorithm)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] hash = algorithm.ComputeHash(data);

            return Convert.ToBase64String(hash);
        }
    }
}
