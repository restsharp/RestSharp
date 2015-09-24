using System;
using System.Text;
#if WINDOWS_UWP
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
#else
using System.Security.Cryptography;
#endif


namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class OAuthExtensions
    {
        public static string ToRequestValue(this OAuthSignatureMethod signatureMethod)
        {
            string value = signatureMethod.ToString()
                .ToUpper();
            int shaIndex = value.IndexOf("SHA1");

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

                case "RSA-SHA1":
                    return OAuthSignatureMethod.RsaSha1;

                default:
                    return OAuthSignatureMethod.PlainText;
            }
        }


        public static string HashWithHMACSHA1(this string input, string key)
        {
#if WINDOWS_UWP
            MacAlgorithmProvider algorithm = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);

            IBuffer keyBuffer = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            CryptographicKey signatureKey = algorithm.CreateKey(keyBuffer);

            IBuffer data = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);

            IBuffer hash = CryptographicEngine.Sign(signatureKey, data);

            return CryptographicBuffer.EncodeToBase64String(hash);
#else
            HMACSHA1 algorithm = new HMACSHA1 {Key = Encoding.UTF8.GetBytes(key)};

            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] hash = algorithm.ComputeHash(data);

            return Convert.ToBase64String(hash);
        
#endif
        }
    }
}