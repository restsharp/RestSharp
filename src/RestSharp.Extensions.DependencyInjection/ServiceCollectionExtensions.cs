using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RestSharp.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions {
    extension(IServiceCollection services) {
        /// <summary>
        /// Adds a named RestClient to the service collection.
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="configureRestClient">Optional: function to configure the client options.</param>
        /// <param name="configureSerialization">Optional: function to configure serializers.</param>
        [PublicAPI]
        public void AddRestClient(
            string                  name,
            ConfigureRestClient?    configureRestClient    = null,
            ConfigureSerialization? configureSerialization = null
        ) {
            Ensure.NotEmptyString(name, nameof(name));
            Ensure.NotNull(services, nameof(services));

            var options   = new RestClientOptions();
            var configure = configureRestClient ?? (_ => { });
            configure(options);

            services
                .AddHttpClient(name)
                .ConfigureHttpClient(client => RestClient.ConfigureHttpClient(client, options))
                .ConfigurePrimaryHttpMessageHandler(() => {
                        var handler = new HttpClientHandler();
                        RestClient.ConfigureHttpMessageHandler(handler, options);
                        return handler;
                    }
                );

            services.TryAddSingleton<IRestClientFactory, DefaultRestClientFactory>();

            if (name == Constants.DefaultRestClient) {
                services.AddTransient<IRestClient>(sp => {
                        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);
                        return new RestClient(client, options);
                    }
                );
            }
            else {
                services.Configure<RestClientConfigOptions>(
                    Constants.GetConfigName(name),
                    o => {
                        o.ConfigureRestClient    = configureRestClient;
                        o.ConfigureSerialization = configureSerialization;
                    }
                );
            }
        }

        /// <summary>
        /// Adds a RestClient to the service collection with default options.
        /// </summary>
        [PublicAPI]
        public void AddRestClient() => services.AddRestClient(Constants.DefaultRestClient);

        /// <summary>
        /// Adds a RestClient to the service collection with a base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL for the RestClient.</param>
        [PublicAPI]
        public void AddRestClient(Uri baseUrl) => services.AddRestClient(Constants.DefaultRestClient, o => o.BaseUrl = baseUrl);

        /// <summary>
        /// Adds a RestClient to the service collection with custom options.
        /// </summary>
        /// <param name="options">Custom options for the RestClient.</param>
        [PublicAPI]
        public void AddRestClient(RestClientOptions options) {
            Ensure.NotNull(options, nameof(options));
            services.AddRestClient(Constants.DefaultRestClient, o => o.CopyFrom(options));
        }

        /// <summary>
        /// Adds a named RestClient to the service collection with base URL.
        /// </summary>
        /// <param name="name">Client name.</param>
        /// <param name="baseUrl">The base URL for the RestClient.</param>
        public void AddRestClient(string name, Uri baseUrl) => services.AddRestClient(name, o => o.BaseUrl = baseUrl);

        /// <summary>
        /// Adds a named RestClient to the service collection with custom options.
        /// </summary>
        /// <param name="name">Client name.</param>
        /// <param name="options">Custom options for the RestClient.</param>
        public void AddRestClient(string name, RestClientOptions options) {
            Ensure.NotNull(options, nameof(options));
            services.AddRestClient(name, o => o.CopyFrom(options));
        }
    }
}