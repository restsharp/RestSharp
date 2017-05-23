using System;
#if !WINDOWS_UWP
using System.Security.Cryptography;
#else
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
#endif
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

#if !WINDOWS_UWP
        public static string HashWith(this string input, HashAlgorithm algorithm)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] hash = algorithm.ComputeHash(data);

            return Convert.ToBase64String(hash);
        }
#else
        public static string HashWith(this string input, HashAlgorithmProvider algorithm)
        {
            CryptographicHash objHash = algorithm.CreateHash();
            IBuffer buffMsg1 = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffMsg1);
            IBuffer buffHash1 = objHash.GetValueAndReset();

            return CryptographicBuffer.EncodeToBase64String(buffHash1);
        }

#endif
    }
}
