using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace RestSharp.Tests.Shared.Fixtures
{
    public class TestRequestHandler
    {
        readonly Regex _comparisonRegex;

        readonly List<string> _urlParameterNames = new List<string>();

        public TestRequestHandler(
            string url,
            string httpMethod,
            Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>> handlerAction
        )
        {
            Url           = url;
            HttpMethod    = httpMethod;
            HandlerAction = handlerAction;

            _comparisonRegex = CreateComparisonRegex(url);
        }

        public TestRequestHandler(string url, Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>> handlerAction)
            : this(url, null, handlerAction) { }

        string Url { get; }
        string HttpMethod { get; }
        internal Action<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>> HandlerAction { get; }

        Regex CreateComparisonRegex(string url)
        {
            var regexString = Regex.Escape(url).Replace(@"\{", "{");

            regexString += regexString.EndsWith("/") ? "?" : "/?";
            regexString =  (regexString.StartsWith("/") ? "^" : "^/") + regexString;

            var regex = new Regex(@"{(.*?)}");

            foreach (Match match in regex.Matches(regexString))
            {
                regexString = regexString.Replace(match.Value, @"(.*?)");
                _urlParameterNames.Add(match.Groups[1].Value);
            }

            regexString += !regexString.Contains(@"\?") ? @"(\?.*)?$" : "$";

            return new Regex(regexString);
        }

        public bool TryMatchUrl(string rawUrl, string httpMethod, out Dictionary<string, string> parameters)
        {
            var match = _comparisonRegex.Match(rawUrl);

            var isMethodMatched = HttpMethod == null || HttpMethod.Split(',').Contains(httpMethod);

            if (!match.Success || !isMethodMatched)
            {
                parameters = null;
                return false;
            }

            parameters = new Dictionary<string, string>();

            for (var i = 0; i < _urlParameterNames.Count; i++)
                parameters[_urlParameterNames[i]] = match.Groups[i + 1].Value;
            return true;
        }
    }
}