using System.Text;

namespace RestSharp.Authenticators; 

/// <summary>
/// Allows "basic access authentication" for HTTP requests.
/// </summary>
/// <remarks>
/// Encoding can be specified depending on what your server expect (see https://stackoverflow.com/a/7243567).
/// UTF-8 is used by default but some servers might expect ISO-8859-1 encoding.
/// </remarks>
[PublicAPI]
public class HttpBasicAuthenticator : AuthenticatorBase
{
    public HttpBasicAuthenticator(string username, string password) : this(username, password, Encoding.UTF8) { }

    public HttpBasicAuthenticator(string username, string password, Encoding encoding)
        : base(GetHeader(username, password, encoding)) { }

    static string GetHeader(string username, string password, Encoding encoding)
        => Convert.ToBase64String(encoding.GetBytes($"{username}:{password}"));

    // return ;
    protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        => new(new Parameter("Authorization", $"Basic {accessToken}", ParameterType.HttpHeader));
}