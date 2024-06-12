namespace RestSharp.Tests.Shared.Extensions; 

public static class UriExtensions {
    public static IDictionary<string, string> ParseQuery(this Uri uri) {
        var query = uri.Query[1..].Split('&');
        return query.Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);
    }
}