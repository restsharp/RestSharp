using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
    public interface IHttpConverter
    {
        IHttpRequest ConvertTo(IRestClient restClient, IRestRequest restRequest);

        IRestResponse ConvertFrom(IHttpResponse httpResponse);
    }
}
