# OAuth2 Token Lifecycle Authenticators

**Issue:** [#2101](https://github.com/restsharp/RestSharp/issues/2101)
**Date:** 2026-03-01

## Problem

The existing OAuth2 authenticators are static token stampers — they take a pre-obtained token and add it to requests. Users who need automatic token acquisition, caching, and refresh hit a circular dependency: the authenticator needs an HttpClient to call the token endpoint, but it lives inside the RestClient it's attached to.

## Solution

Self-contained OAuth2 authenticators that manage the full token lifecycle using their own internal HttpClient for token endpoint calls.

## Components

### OAuth2TokenResponse

RFC 6749 Section 5.1 token response model. Used for deserializing token endpoint responses.

Fields: `AccessToken`, `TokenType`, `ExpiresIn`, `RefreshToken` (optional), `Scope` (optional). Deserialized with `System.Text.Json` using `JsonPropertyName` attributes for snake_case mapping.

### OAuth2TokenRequest

Shared configuration for token endpoint calls.

- `TokenEndpointUrl` (required) — URL of the OAuth2 token endpoint
- `ClientId` (required) — OAuth2 client ID
- `ClientSecret` (required) — OAuth2 client secret
- `Scope` (optional) — requested scope
- `ExtraParameters` (optional) — additional form parameters
- `HttpClient` (optional) — bring your own HttpClient for token calls
- `ExpiryBuffer` — refresh before actual expiry (default 30s)
- `OnTokenRefreshed` — callback fired when a new token is obtained

### OAuth2Token

Simple record `(string AccessToken, DateTimeOffset ExpiresAt)` for the generic authenticator's delegate return type.

### OAuth2ClientCredentialsAuthenticator

Machine-to-machine flow. POSTs `grant_type=client_credentials` to the token endpoint. Caches the token and refreshes when expired. Thread-safe via SemaphoreSlim with double-check pattern. Implements IDisposable to clean up owned HttpClient.

### OAuth2RefreshTokenAuthenticator

User token flow. Takes initial access + refresh tokens. When the access token expires, POSTs `grant_type=refresh_token`. Updates the cached refresh token if the server rotates it. Fires `OnTokenRefreshed` callback so callers can persist new tokens.

### OAuth2TokenAuthenticator

Generic/delegate-based. Takes `Func<CancellationToken, Task<OAuth2Token>>`. For non-standard flows where users provide their own token acquisition logic. Caches the result and re-invokes the delegate on expiry.

## Data Flow

```
Request → Authenticate()
  → cached token valid? → stamp Authorization header
  → expired? → acquire SemaphoreSlim
    → double-check still expired
    → POST to token endpoint (own HttpClient)
    → parse OAuth2TokenResponse
    → cache token, compute expiry (ExpiresIn - ExpiryBuffer)
    → fire OnTokenRefreshed callback
    → stamp Authorization header
```

## Error Handling

- Non-2xx from token endpoint: throw HttpRequestException with status and body
- Missing access_token in response: throw InvalidOperationException
- No retry logic — callers control retries at RestClient level

## Thread Safety

SemaphoreSlim(1, 1) with double-check pattern. One thread refreshes; concurrent callers wait and reuse the new token.

## IDisposable

Authenticators that create their own HttpClient dispose it. User-provided HttpClient is not disposed. Same pattern as RestClient itself.

## Multi-targeting

System.Text.Json for deserialization — NuGet package on netstandard2.0/net471/net48, built-in on net8.0+. No conditional compilation needed.

## Files

```
src/RestSharp/Authenticators/OAuth2/
  OAuth2TokenResponse.cs                    (new)
  OAuth2TokenRequest.cs                     (new)
  OAuth2Token.cs                            (new)
  OAuth2ClientCredentialsAuthenticator.cs   (new)
  OAuth2RefreshTokenAuthenticator.cs        (new)
  OAuth2TokenAuthenticator.cs               (new)

test/RestSharp.Tests/Auth/
  OAuth2ClientCredentialsAuthenticatorTests.cs  (new)
  OAuth2RefreshTokenAuthenticatorTests.cs       (new)
  OAuth2TokenAuthenticatorTests.cs              (new)
```

No changes to existing files. No API breaks.
