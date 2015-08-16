using System;

namespace RestSharp.Authenticators.OAuth
{
#if !SILVERLIGHT && !WINDOWS_PHONE
    [Serializable]
#endif
    internal class WebParameter : WebPair
    {
        public WebParameter(string name, string value)
            : base(name, value) { }
    }
}
