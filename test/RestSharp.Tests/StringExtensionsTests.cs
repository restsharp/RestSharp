using System.Globalization;
using System.Text;
using RestSharp.Extensions;

namespace RestSharp.Tests;

public class StringExtensionsTests {
    [Fact]
    public void UrlEncode_Throws_ArgumentNullException_For_Null_Input() {
        string nullString = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        Assert.Throws<ArgumentNullException>(() => nullString!.UrlEncode());
    }

    [Fact]
    public void UrlEncode_Returns_Correct_Length_When_Less_Than_Limit() {
        const int numLessThanLimit      = 32766;
        var       stringWithLimitLength = new string('*', numLessThanLimit);
        var       encodedAndDecoded     = stringWithLimitLength.UrlEncode().UrlDecode();
        Assert.Equal(numLessThanLimit, encodedAndDecoded.Length);
    }

    [Fact]
    public void UrlEncode_Returns_Correct_Length_When_More_Than_Limit() {
        const int numGreaterThanLimit   = 65000;
        var       stringWithLimitLength = new string('*', numGreaterThanLimit);
        var       encodedAndDecoded     = stringWithLimitLength.UrlEncode().UrlDecode();
        Assert.Equal(numGreaterThanLimit, encodedAndDecoded.Length);
    }

    [Fact]
    public void UrlEncode_Does_Not_Fail_When_4_Byte_Unicode_Character_Lies_Between_Chunks() {
        var stringWithLimitLength = new string('*', 32765);
        stringWithLimitLength += "😉*****"; // 2 + 5 chars
        var encodedAndDecoded = stringWithLimitLength.UrlEncode().UrlDecode();
        Assert.Equal(stringWithLimitLength, encodedAndDecoded);

        // now between another 2 chunks
        stringWithLimitLength =  new string('*', 32766 * 2 - 1);
        stringWithLimitLength += "😉*****"; // 2 + 5 chars
        encodedAndDecoded     =  stringWithLimitLength.UrlEncode().UrlDecode();
        Assert.Equal(stringWithLimitLength, encodedAndDecoded);
    }

    [Fact]
    public void UrlEncodeTest() {
        const string parameter = "ø";
        Assert.Equal("%F8", parameter.UrlEncode(Encoding.GetEncoding("ISO-8859-1")), true);
        Assert.Equal("%C3%B8", parameter.UrlEncode(), true);
    }

    [Theory]
    [InlineData("this_is_a_test", true, "ThisIsATest")]
    [InlineData("this_is_a_test", false, "This_Is_A_Test")]
    public void ToPascalCase(string start, bool removeUnderscores, string finish) {
        var result = start.ToPascalCase(removeUnderscores, CultureInfo.InvariantCulture);

        Assert.Equal(finish, result);
    }

    [Theory]
    [InlineData("DueDate", "dueDate")]
    [InlineData("ID", "id")]
    [InlineData("IDENTIFIER", "identifier")]
    [InlineData("primaryId", "primaryId")]
    [InlineData("A", "a")]
    [InlineData("ThisIsATest", "thisIsATest")]
    public void ToCamelCase(string start, string finish) {
        var result = start.ToCamelCase(CultureInfo.InvariantCulture);

        Assert.Equal(finish, result);
    }
}