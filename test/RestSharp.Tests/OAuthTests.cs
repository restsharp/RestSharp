﻿using System.Globalization;
using System.Security.Cryptography;
using RestSharp.Authenticators.OAuth;
using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp.Tests; 

public class OAuthTests {
    public OAuthTests() {
        Thread.CurrentThread.CurrentCulture   = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
    }

    [Fact]
    public void HmacSha256_Does_Not_Accept_Nulls() {
        const string consumerSecret = "12345678";

        Assert.Throws<ArgumentNullException>(
            () => OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha256, null, consumerSecret)
        );
    }

    [Theory]
    [InlineData(
        "The quick brown fox jumps over the lazy dog",
        "rVL90tHhGt0eQ0TCITY74nVL22P%2FltlWS7WvJXpECPs%3D",
        "12345678"
    )]
    [InlineData(
        "The quick\tbrown\nfox\rjumps\r\nover\t\tthe\n\nlazy\r\n\r\ndog",
        "C%2B2RY0Hna6VrfK1crCkU%2FV1e0ECoxoDh41iOOdmEMx8%3D",
        "12345678"
    )]
    [InlineData("", "%2BnkCwZfv%2FQVmBbNZsPKbBT3kAg3JtVn3f3YMBtV83L8%3D", "12345678")]
    [InlineData(" !\"#$%&'()*+,", "xcTgWGBVZaw%2Bilg6kjWAGt%2FhCcsVBMMe1CcDEnxnh8Y%3D", "12345678")]
    [InlineData("AB", "JJgraAxzpO2Q6wiC3blM4eiQeA9WmkALaZI8yGRH4qM%3D", "CD!")]
    public void HmacSha256_Hashes_Correctly(string value, string expected, string consumerSecret) {
        var actual = OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha256, value, consumerSecret);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("1234", "%31%32%33%34")]
    [InlineData("\x00\x01\x02\x03", "%00%01%02%03")]
    [InlineData("\r\n\t", "%0D%0A%09")]
    public void PercentEncode_Encodes_Correctly(string value, string expected) {
        var actual = value.PercentEncode();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("The quick brown fox jumps over the lazy dog", 1024)]
    [InlineData("The quick brown fox jumps over the lazy dog", 2048)]
    [InlineData("The quick brown fox jumps over the lazy dog", 4096)]
    [InlineData("", 2048)]
    [InlineData(" !\"#$%&'()*+,", 2048)]
    public void RsaSha1_Signs_Correctly(string value, int keySize) {
        var hasher = SHA1.Create();
        var hash   = hasher.ComputeHash(value.GetBytes());

        using var crypto = new RSACryptoServiceProvider(keySize) { PersistKeyInCsp = false };

        var privateKey = crypto.ToXmlString(true);

        var signature = OAuthTools.GetSignature(
            OAuthSignatureMethod.RsaSha1,
            OAuthSignatureTreatment.Unescaped,
            value,
            privateKey
        );

        var signatureBytes = Convert.FromBase64String(signature);

        Assert.True(crypto.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signatureBytes));
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("0123456789", "0123456789")]
    [InlineData("-._~", "-._~")]
    [InlineData(" !\"#$%&'()*+,", "%20%21%22%23%24%25%26%27%28%29%2A%2B%2C")]
    [InlineData("%$%", "%25%24%25")]
    [InlineData("%", "%25")]
    [InlineData("/:;<=>?@", "%2F%3A%3B%3C%3D%3E%3F%40")]
    [InlineData("\x00\x01\a\b\f\n\r\t\v", @"%00%01%07%08%0C%0A%0D%09%0B")]
    public void UrlStrictEncode_Encodes_Correctly(string value, string expected) {
        var actual = OAuthTools.UrlEncodeStrict(value);

        Assert.Equal(expected, actual);
    }
}