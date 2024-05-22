namespace RestSharp.Tests;

public class RequestHeaderTests {
    static readonly KeyValuePair<string, string> AcceptHeader         = new(KnownHeaders.Accept, ContentType.Json);
    static readonly KeyValuePair<string, string> AcceptLanguageHeader = new(KnownHeaders.AcceptLanguage, "en-us,en;q=0.5");
    static readonly KeyValuePair<string, string> KeepAliveHeader      = new(KnownHeaders.KeepAlive, "300");

    readonly List<KeyValuePair<string, string>> _headers = [AcceptHeader, AcceptLanguageHeader, KeepAliveHeader];

    [Fact]
    public void AddHeaders_SameCaseDuplicatesExist_ThrowsException() {
        var headers = _headers;
        _headers.Add(AcceptHeader);

        var request = new RestRequest();

        var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
        Assert.Equal("Duplicate header names exist: ACCEPT", exception.Message);
    }

    [Fact]
    public void AddHeaders_DifferentCaseDuplicatesExist_ThrowsException() {
        var headers = _headers;
        headers.Add(new KeyValuePair<string, string>(KnownHeaders.Accept, ContentType.Json));

        var request = new RestRequest();

        var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
        Assert.Equal("Duplicate header names exist: ACCEPT", exception.Message);
    }

    [Fact]
    public void AddHeaders_NoDuplicatesExist_Has3Headers() {
        var request = new RestRequest();
        request.AddHeaders(_headers);

        var httpParameters = GetHeaders(request);
        Assert.Equal(3, httpParameters.Count());
    }

    [Fact]
    public void AddHeaders_NoDuplicatesExistUsingDictionary_Has3Headers() {
        var headers = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Json },
            { KnownHeaders.AcceptLanguage, "en-us,en;q=0.5" },
            { KnownHeaders.KeepAlive, "300" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        var httpParameters = GetHeaders(request);
        Assert.Equal(3, httpParameters.Count());
    }

    [Fact]
    public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderExist() {
        // Arrange
        var request = new RestRequest();
        request.AddHeader(KnownHeaders.Accept, ContentType.Xml);

        // Act
        request.AddOrUpdateHeader(KnownHeaders.Accept, ContentType.Json);

        // Assert
        var headers = GetHeaders(request);
        GetHeader(request, KnownHeaders.Accept).Should().Be(ContentType.Json);
    }

    [Fact]
    public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderDoesNotExist() {
        // Arrange
        var request = new RestRequest();

        // Act
        request.AddOrUpdateHeader(KnownHeaders.Accept, ContentType.Json);

        // Assert
        GetHeader(request, KnownHeaders.Accept).Should().Be(ContentType.Json);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldAddHeaders_WhenNoneExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Json },
            { KnownHeaders.AcceptLanguage, "en-us,en;q=0.5" },
            { KnownHeaders.KeepAlive, "300" }
        };

        var request = new RestRequest();

        // Act
        request.AddOrUpdateHeaders(headers);

        // Assert
        var requestHeaders = GetHeaders(request);

        var expected = headers.Select(x => new HeaderParameter(x.Key, x.Value));
        expected.Should().BeEquivalentTo(requestHeaders);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldUpdateHeaders_WhenAllExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Json },
            { KnownHeaders.KeepAlive, "300" }
        };

        var updatedHeaders = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Xml },
            { KnownHeaders.KeepAlive, "400" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        // Act
        request.AddOrUpdateHeaders(updatedHeaders);

        // Assert
        var requestHeaders = GetHeaders(request);
        HeaderParameter[] expected = [
            new HeaderParameter(KnownHeaders.Accept, ContentType.Xml),
            new HeaderParameter(KnownHeaders.KeepAlive, "400")
        ];
        requestHeaders.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldAddAndUpdateHeaders_WhenSomeExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Json },
            { KnownHeaders.KeepAlive, "300" }
        };

        var updatedHeaders = new Dictionary<string, string> {
            { KnownHeaders.Accept, ContentType.Xml },
            { KnownHeaders.AcceptLanguage, "en-us,en;q=0.5" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        // Act
        request.AddOrUpdateHeaders(updatedHeaders);

        // Assert
        var requestHeaders = GetHeaders(request);

        HeaderParameter[] expected = [
            new HeaderParameter(KnownHeaders.Accept, ContentType.Xml),
            new HeaderParameter(KnownHeaders.AcceptLanguage, "en-us,en;q=0.5"),
            new HeaderParameter(KnownHeaders.KeepAlive, "300")
        ];
        requestHeaders.Should().BeEquivalentTo(expected);
    }

    static Parameter[] GetHeaders(RestRequest request) => request.Parameters.Where(x => x.Type == ParameterType.HttpHeader).ToArray();

    static string GetHeader(RestRequest request, string name) => request.Parameters.FirstOrDefault(x => x.Name == name)?.Value?.ToString();
}