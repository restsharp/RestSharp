//  Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using RestSharp.Authenticators;

namespace RestSharp.InteractiveTests;

public interface ITwitterClient {
    Task<TwitterUser> GetUser(string user);
}

public class TwitterClient : ITwitterClient, IDisposable {
    readonly RestClient     _client;
    readonly IAuthenticator _authenticator;

    public TwitterClient(string apiKey, string apiKeySecret) {
        var options = new RestClientOptions("https://api.twitter.com/2");

        _client        = new RestClient(options);
        _authenticator = new TwitterAuthenticator("https://api.twitter.com", apiKey, apiKeySecret);
    }

    public async Task<TwitterUser> GetUser(string user) {
        var response = await _client.GetJsonAsync<TwitterSingleObject<TwitterUser>>(
            "users/by/username/{user}",
            new { user },
            authenticator: _authenticator
        );
        return response!.Data;
    }

    public async Task<SearchRulesResponse[]> AddSearchRules(params AddStreamSearchRule[] rules) {
        var response = await _client.PostJsonAsync<AddSearchRulesRequest, TwitterCollectionObject<SearchRulesResponse>>(
            "tweets/search/stream/rules",
            new AddSearchRulesRequest(rules),
            authenticator: _authenticator
        );
        return response?.Data;
    }

    public async Task<SearchRulesResponse[]> GetSearchRules() {
        var response = await _client.GetJsonAsync<TwitterCollectionObject<SearchRulesResponse>>(
            "tweets/search/stream/rules",
            authenticator: _authenticator);
        return response?.Data;
    }

    public async IAsyncEnumerable<SearchResponse> SearchStream([EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var response = _client.StreamJsonAsync<TwitterSingleObject<SearchResponse>>(
            "tweets/search/stream",
            cancellationToken,
            _authenticator);

        await foreach (var item in response.WithCancellation(cancellationToken)) {
            yield return item.Data;
        }
    }

    record TwitterSingleObject<T>(T Data);

    record TwitterCollectionObject<T>(T[] Data);

    record AddSearchRulesRequest(AddStreamSearchRule[] Add);

    public void Dispose() {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

class TwitterAuthenticator : AuthenticatorBase {
    readonly string _baseUrl;
    readonly string _clientId;
    readonly string _clientSecret;

    public TwitterAuthenticator(string baseUrl, string clientId, string clientSecret) : base("") {
        _baseUrl      = baseUrl;
        _clientId     = clientId;
        _clientSecret = clientSecret;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken) {
        var token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        Token = token;
        return new HeaderParameter(KnownHeaders.Authorization, token);
    }

    async Task<string> GetToken() {
        var options = new RestClientOptions(_baseUrl);

        using var client = new RestClient(options);

        var request = new RestRequest("oauth2/token") {
            Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
        }.AddParameter("grant_type", "client_credentials");
        var response = await client.PostAsync<TokenResponse>(request);
        return $"{response!.TokenType} {response!.AccessToken}";
    }

    record TokenResponse {
        [JsonPropertyName("token_type")]
        public string TokenType { get; init; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; }
    }
}

public record TwitterUser(string Id, string Name, string Username);

public record AddStreamSearchRule(string Value, string Tag);

public record SearchRulesResponse(string Value, string Tag, string Id);

public record SearchResponse(string Id, string Text);