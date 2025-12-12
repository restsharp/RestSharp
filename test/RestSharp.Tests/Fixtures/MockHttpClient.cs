using RichardSzalay.MockHttp;

namespace RestSharp.Tests.Fixtures;

public static class MockHttpClient {
    const string Url = "https://dummy.org";

    public static RestClient Create(Method method, Func<MockedRequest, MockedRequest> configureHandler, ConfigureRestClient configure = null) {
        var mockHttp = new MockHttpMessageHandler();
        configureHandler(mockHttp.When(RestClient.AsHttpMethod(method), Url));

        var options = new RestClientOptions(Url) {
            ConfigureMessageHandler = _ => mockHttp
        };
        configure?.Invoke(options);
        return new RestClient(options);
    }
}