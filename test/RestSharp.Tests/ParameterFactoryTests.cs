using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
namespace RestSharp.Tests
{
    [TestFixture]
    public class ParameterFactoryTests
    {
        const string Name = "name";
        const string ValueString = "value";
        const string ContentType = "plain/text";

        private static IEnumerable<TestCaseData> CreateWithoutContentType()
        {
            yield return new TestCaseData(ParameterType.Cookie, new CookieParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.GetOrPost, new GetOrPostParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.UrlSegment, new UrlSegmentParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.HttpHeader, new HttpHeaderParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.RequestBody, new BodyParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.QueryString, new QueryStringParameter(Name, ValueString, true));
            yield return new TestCaseData(ParameterType.QueryStringWithoutEncode, new QueryStringParameter(Name, ValueString, false));
        }
        [Test, TestCaseSource(nameof(CreateWithoutContentType))]
        public void Create_ShouldCreateCorrectParameter_WhenNoContentTypeGiven(ParameterType type, Parameter expected)
        {
            var result = ParameterFactory.Create(Name, ValueString, type);

            result.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> CreateWithContentType()
        {
            yield return new TestCaseData(ParameterType.Cookie, new CookieParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.GetOrPost, new GetOrPostParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.UrlSegment, new UrlSegmentParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.HttpHeader, new HttpHeaderParameter(Name, ValueString));
            yield return new TestCaseData(ParameterType.RequestBody, new BodyParameter(Name, ValueString, ContentType));
            yield return new TestCaseData(ParameterType.QueryString, new QueryStringParameter(Name, ValueString, true));
            yield return new TestCaseData(ParameterType.QueryStringWithoutEncode, new QueryStringParameter(Name, ValueString, false));
        }
        [Test, TestCaseSource(nameof(CreateWithContentType))]
        public void Create_ShouldCreateCorrectParameter_WhenContentTypeGiven(ParameterType type, Parameter expected)
        {
            var result = ParameterFactory.Create(Name, ValueString, ContentType, type);

            result.Should().BeEquivalentTo(expected);
        }
    }
}