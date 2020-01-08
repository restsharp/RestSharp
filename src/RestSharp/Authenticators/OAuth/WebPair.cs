using System.Collections.Generic;

namespace RestSharp.Authenticators.OAuth
{
    internal class WebPair
    {
        public WebPair(string name, string value)
        {
            Name  = name;
            Value = value;
        }

        public string Value { get; }

        public string Name { get; }
        
        internal static WebPairComparer Comparer { get; } = new WebPairComparer();

        internal class WebPairComparer : IComparer<WebPair>
        {
            public int Compare(WebPair x, WebPair y)
            {
                var compareName = string.CompareOrdinal(x?.Name, y?.Name);

                return compareName != 0 ? compareName : string.CompareOrdinal(x?.Value, y?.Value);
            }
        }
    }
}