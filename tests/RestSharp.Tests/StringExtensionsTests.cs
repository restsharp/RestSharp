using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RestSharp.Extensions;
using Xunit;

namespace RestSharp.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void UrlEncode_Throws_ArgumentNullException_For_Null_Input()
        {
            const string nullString = null;
            Assert.Throws<System.ArgumentNullException>(
                delegate
                {
                    nullString.UrlEncode();
                });   
        }

        [Fact]
        public void UrlEncode_Returns_Correct_Length_When_Less_Than_Limit()
        {
            const int numLessThanLimit = 32766;
            string stringWithLimitLength = new string('a', numLessThanLimit);
            Assert.Equal(numLessThanLimit, stringWithLimitLength.UrlEncode().Length);
        }

        [Fact]
        public void UrlEncode_Returns_Correct_Length_When_More_Than_Limit()
        {
            const int numGreaterThanLimit = 65000;
            string stringWithLimitLength = new string('a', numGreaterThanLimit);
            Assert.Equal(numGreaterThanLimit, stringWithLimitLength.UrlEncode().Length);
        }
    }
}
