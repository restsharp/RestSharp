using Microsoft.Extensions.DependencyInjection;

namespace RestSharp.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions {
    const string DefaultRestClient = "DefaultRestClient";

    extension(IServiceCollection services) {
        /// <summary>
        /// Adds a RestClient to the service collection.
        /// </summary>
        /// <param name="options">The configuration options for the RestClient.</param>
        [PublicAPI]
        public void AddRestClient(RestClientOptions options) {
            services
                .AddHttpClient(DefaultRestClient)
                .ConfigureHttpClient(client => RestClient.ConfigureHttpClient(client, options))
                .ConfigurePrimaryHttpMessageHandler(() => {
                        var handler = new HttpClientHandler();
                        RestClient.ConfigureHttpMessageHandler(handler, options);
                        return handler;
                    }
                );

            services.AddTransient<IRestClient>(sp => {
                    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(DefaultRestClient);
                    return new RestClient(client, options);
                }
            );
        }

        /// <summary>
        /// Adds a RestClient to the service collection with default options.
        /// </summary>
        [PublicAPI]
        public void AddRestClient() => services.AddRestClient(new RestClientOptions());

        /// <summary>
        /// Adds a RestClient to the service collection with a base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL for the RestClient.</param>
        [PublicAPI]
        public void AddRestClient(string baseUrl) => services.AddRestClient(new RestClientOptions(baseUrl));
    }
}