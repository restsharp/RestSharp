using System;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Extensions;

namespace RestSharp.Tests
{
    public class StringExtensionsTests
    {
        [Test]
        public void UrlEncode_Throws_ArgumentNullException_For_Null_Input()
        {
            const string nullString = null;

            Assert.Throws<ArgumentNullException>(() => nullString.UrlEncode());
        }

        [Test]
        public void UrlEncode_Returns_Correct_Length_When_Less_Than_Limit()
        {
            const int numLessThanLimit      = 32766;
            var       stringWithLimitLength = new string('*', numLessThanLimit);
            var       encodedAndDecoded     = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(numLessThanLimit, encodedAndDecoded.Length);
        }

        [Test]
        public void UrlEncode_Returns_Correct_Length_When_More_Than_Limit()
        {
            const int numGreaterThanLimit   = 65000;
            var       stringWithLimitLength = new string('*', numGreaterThanLimit);
            var       encodedAndDecoded     = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(numGreaterThanLimit, encodedAndDecoded.Length);
        }

        [Test]
        public void UrlEncode_Does_Not_Fail_When_4_Byte_Unicode_Character_Lies_Between_Chunks()
        {
            var stringWithLimitLength = new string('*', 32765);
            stringWithLimitLength += "😉*****"; // 2 + 5 chars
            var encodedAndDecoded = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(stringWithLimitLength, encodedAndDecoded);

            // now between another 2 chunks
            stringWithLimitLength = new string('*', 32766 * 2 - 1);
            stringWithLimitLength += "😉*****"; // 2 + 5 chars
            encodedAndDecoded = stringWithLimitLength.UrlEncode().UrlDecode();
            Assert.AreEqual(stringWithLimitLength, encodedAndDecoded);
        }

        [Test]
        public void UrlEncodeTest()
        {
            const string parameter = "ø";
            Assert.True(string.Equals("%F8", parameter.UrlEncode(Encoding.GetEncoding("ISO-8859-1")), StringComparison.OrdinalIgnoreCase));
            Assert.True(string.Equals("%C3%B8", parameter.UrlEncode(), StringComparison.OrdinalIgnoreCase));
        }

        [Test, TestCase("this_is_a_test", true, "ThisIsATest"), TestCase("this_is_a_test", false, "This_Is_A_Test")]
        public void ToPascalCase(string start, bool removeUnderscores, string finish)
        {
            var result = start.ToPascalCase(removeUnderscores, CultureInfo.InvariantCulture);

            Assert.AreEqual(finish, result);
        }

        [Test]
        public void Does_not_throw_on_invalid_encoding()
        {
            const string value = "SomeValue";
            var bytes = Encoding.UTF8.GetBytes(value);

            var decoded = bytes.AsString("blah");
            decoded.Should().Be(value);
        }
        
        [Test]
        public void Does_not_throw_on_missing_encoding()
        {
            const string value = "SomeValue";
            var bytes = Encoding.UTF8.GetBytes(value);

            var decoded = bytes.AsString(null);
            decoded.Should().Be(value);
        }
    }
}