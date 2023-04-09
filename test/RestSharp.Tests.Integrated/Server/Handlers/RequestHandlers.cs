using Microsoft.AspNetCore.Http;

namespace RestSharp.Tests.Integrated.Server.Handlers;

public class ParsedRequest {
    public ParsedRequest(HttpRequest request) {
        Method      = request.Method;
        Path        = request.Path;
        QueryString = request.QueryString;

        QueryParameters = request.Query
            .SelectMany(x => x.Value.Select(y => new KeyValuePair<string, string>(x.Key, y!)))
            .ToArray();
    }

    public string                         Method          { get; set; }
    public string                         Path            { get; set; }
    public QueryString                    QueryString     { get; set; }
    public KeyValuePair<string, string>[] QueryParameters { get; set; }
}
