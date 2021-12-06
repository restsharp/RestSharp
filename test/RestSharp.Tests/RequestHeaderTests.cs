namespace RestSharp.Tests; 

public class RequestHeaderTests {
    [Fact]
    public void AddHeaders_SameCaseDuplicatesExist_ThrowsException() {
        var headers = new List<KeyValuePair<string, string>> {
            new("Accept", "application/json"),
            new("Accept-Language", "en-us,en;q=0.5"),
            new("Keep-Alive", "300"),
            new("Accept", "application/json")
        };

        var request = new RestRequest();

        var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
        Assert.Equal("Duplicate header names exist: ACCEPT", exception.Message);
    }

    [Fact]
    public void AddHeaders_DifferentCaseDuplicatesExist_ThrowsException() {
        var headers = new List<KeyValuePair<string, string>> {
            new("Accept", "application/json"),
            new("Accept-Language", "en-us,en;q=0.5"),
            new("Keep-Alive", "300"),
            new("acCEpt", "application/json")
        };

        var request = new RestRequest();

        var exception = Assert.Throws<ArgumentException>(() => request.AddHeaders(headers));
        Assert.Equal("Duplicate header names exist: ACCEPT", exception.Message);
    }

    [Fact]
    public void AddHeaders_NoDuplicatesExist_Has3Headers() {
        var headers = new List<KeyValuePair<string, string>> {
            new("Accept", "application/json"),
            new("Accept-Language", "en-us,en;q=0.5"),
            new("Keep-Alive", "300")
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        var httpParameters = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader);

        Assert.Equal(3, httpParameters.Count());
    }

    [Fact]
    public void AddHeaders_NoDuplicatesExistUsingDictionary_Has3Headers() {
        var headers = new Dictionary<string, string> {
            { "Accept", "application/json" },
            { "Accept-Language", "en-us,en;q=0.5" },
            { "Keep-Alive", "300" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        var httpParameters = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader);

        Assert.Equal(3, httpParameters.Count());
    }

    [Fact]
    public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderExist() {
        // Arrange
        var request = new RestRequest();
        request.AddHeader("Accept", "application/xml");

        // Act
        request.AddOrUpdateHeader("Accept", "application/json");

        // Assert
        var headers = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();

        Assert.Equal("application/json", headers.First(parameter => parameter.Name == "Accept").Value);
        Assert.Single(headers);
    }

    [Fact]
    public void AddOrUpdateHeader_ShouldUpdateExistingHeader_WhenHeaderDoesNotExist() {
        // Arrange
        var request = new RestRequest();

        // Act
        request.AddOrUpdateHeader("Accept", "application/json");

        // Assert
        var headers = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();

        Assert.Equal("application/json", headers.First(parameter => parameter.Name == "Accept").Value);
        Assert.Single(headers);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldAddHeaders_WhenNoneExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { "Accept", "application/json" },
            { "Accept-Language", "en-us,en;q=0.5" },
            { "Keep-Alive", "300" }
        };

        var request = new RestRequest();

        // Act
        request.AddOrUpdateHeaders(headers);

        // Assert
        var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();

        Assert.Equal("application/json", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
        Assert.Equal("en-us,en;q=0.5", requestHeaders.First(parameter => parameter.Name == "Accept-Language").Value);
        Assert.Equal("300", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
        Assert.Equal(3, requestHeaders.Length);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldUpdateHeaders_WhenAllExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { "Accept", "application/json" },
            { "Keep-Alive", "300" }
        };

        var updatedHeaders = new Dictionary<string, string> {
            { "Accept", "application/xml" },
            { "Keep-Alive", "400" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        // Act
        request.AddOrUpdateHeaders(updatedHeaders);

        // Assert
        var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();

        Assert.Equal("application/xml", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
        Assert.Equal("400", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
        Assert.Equal(2, requestHeaders.Length);
    }

    [Fact]
    public void AddOrUpdateHeaders_ShouldAddAndUpdateHeaders_WhenSomeExists() {
        // Arrange
        var headers = new Dictionary<string, string> {
            { "Accept", "application/json" },
            { "Keep-Alive", "300" }
        };

        var updatedHeaders = new Dictionary<string, string> {
            { "Accept", "application/xml" },
            { "Accept-Language", "en-us,en;q=0.5" }
        };

        var request = new RestRequest();
        request.AddHeaders(headers);

        // Act
        request.AddOrUpdateHeaders(updatedHeaders);

        // Assert
        var requestHeaders = request.Parameters.Where(parameter => parameter.Type == ParameterType.HttpHeader).ToArray();

        Assert.Equal("application/xml", requestHeaders.First(parameter => parameter.Name == "Accept").Value);
        Assert.Equal("en-us,en;q=0.5", requestHeaders.First(parameter => parameter.Name == "Accept-Language").Value);
        Assert.Equal("300", requestHeaders.First(parameter => parameter.Name == "Keep-Alive").Value);
        Assert.Equal(3, requestHeaders.Length);
    }
}