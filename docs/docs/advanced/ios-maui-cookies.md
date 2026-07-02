---
title: iOS / MAUI Cookie Handling
---

On iOS and Mac Catalyst, `Set-Cookie` response headers are silently absent from `RestResponse.Cookies` and from raw response headers, even though the same request works correctly on Android, Windows, and Linux.

## Root cause

Apple's networking stack, `NSURLSession`, intercepts `Set-Cookie` headers before they reach .NET's `HttpClient`. The cookies are stored in `NSHTTPCookieStorage` instead of being forwarded as headers, so RestSharp never sees them.

## Fix

Disable `NSURLSession`'s automatic cookie storage by supplying a custom session configuration via [`ConfigureMessageHandler`](configuration.md#using-custom-message-handler):

```csharp
#if IOS || MACCATALYST
using Foundation;

var options = new RestClientOptions(baseUrl) {
    ConfigureMessageHandler = _ => {
        var config = NSUrlSessionConfiguration.DefaultSessionConfiguration;
        config.HttpCookieStorage      = null;
        config.HttpCookieAcceptPolicy = NSHttpCookieAcceptPolicy.Never;
        return new NSUrlSessionHandler(config);
    }
};
#endif
```

With this configuration, `NSURLSession` passes `Set-Cookie` headers through to .NET unchanged. RestSharp captures them in `RestResponse.Cookies` as it does on all other platforms.

:::warning This replaces RestSharp's configured handler
Returning a new `NSUrlSessionHandler` from `ConfigureMessageHandler` **replaces** the `HttpClientHandler` that RestSharp already configured from `RestClientOptions` — it does not wrap or extend it. Any handler-level options you set on `RestClientOptions` will **not** apply to the new handler, including:

- `Proxy`
- `Credentials` / `UseDefaultCredentials`
- `AutomaticDecompression`
- `RemoteCertificateValidationCallback`
- `ClientCertificates`

If your app relies on any of these, re-apply the equivalent configuration directly on the `NSUrlSessionHandler` instance (or on the `NSUrlSessionConfiguration`) before returning it.
:::

## Multi-tenant safety

Disabling the system cookie store is the correct approach for API clients that serve multiple users or tenants. When `NSHTTPCookieStorage` is active, cookies from one user's session can leak into a subsequent request made by the same client instance. Opting out gives RestSharp full control: cookies are scoped to the individual request via the per-request [`CookieContainer`](../usage/request.md#cookies), and nothing is persisted outside that scope.

:::warning Anti-pattern: shared CookieContainer with UseCookies = true
Do **not** set `HttpClientHandler.UseCookies = true` with a shared `CookieContainer` on the handler. This pools cookies across every request made by the client, which is unsafe for any multi-tenant scenario on any platform.

RestSharp deliberately avoids this pattern. Cookies are managed at the request level; see [Cookies](../usage/request.md#cookies) for details.
:::