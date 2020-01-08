namespace RestSharp.Authenticators.OAuth
{
    public enum OAuthSignatureMethod { HmacSha1, HmacSha256, PlainText, RsaSha1 }
    
    public enum OAuthSignatureTreatment { Escaped, Unescaped }
    
    public enum OAuthParameterHandling { HttpAuthorizationHeader, UrlOrPostParameters }
    
    public enum OAuthType { RequestToken, AccessToken, ProtectedResource, ClientAuthentication }
}