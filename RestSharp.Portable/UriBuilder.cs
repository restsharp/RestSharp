using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp.Extensions;

namespace RestSharp
{
    public class UriBuilder
    {
        /// <summary>
        /// Assembles URL to call based on parameters, method and resource
        /// </summary>
        /// <param name="request">RestRequest to execute</param>
        /// <returns>Assembled System.Uri</returns>
        public static Uri Build(string baseUrl, IRestRequest request)
        {
            var assembled = request.Resource;
            var urlParms = request.Parameters.Where(p => p.Type == ParameterType.UrlSegment);
            foreach (var p in urlParms)
            {
                assembled = assembled.Replace("{" + p.Name + "}", p.Value.ToString().UrlEncode());
            }

            if (!string.IsNullOrEmpty(assembled) && assembled.StartsWith("/"))
            {
                assembled = assembled.Substring(1);
            }

            if (!string.IsNullOrEmpty(baseUrl))
            {
                if (string.IsNullOrEmpty(assembled))
                {
                    assembled = baseUrl;
                }
                else
                {
                    assembled = string.Format("{0}/{1}", baseUrl, assembled);
                }
            }

            IEnumerable<Parameter> parameters = null;

            if (request.Method != Method.POST && request.Method != Method.PUT && request.Method != Method.PATCH)
            {
                // build and attach querystring if this is a get-style request
                parameters = request.Parameters.Where(p => p.Type == ParameterType.GetOrPost || p.Type == ParameterType.QueryString);
            }
            else
            {
                parameters = request.Parameters.Where(p => p.Type == ParameterType.QueryString);
            }

            // build and attach querystring 
            if (parameters != null && parameters.Any())
            {
                var data = EncodeParameters(parameters);
                assembled = string.Format("{0}?{1}", assembled, data);
            }

            return new Uri(assembled); //would this throw an invalidoperationexception

        }

        private static string EncodeParameters(IEnumerable<Parameter> parameters)
        {
            var querystring = new StringBuilder();
            foreach (var p in parameters)
            {
                if (querystring.Length > 1)
                    querystring.Append("&");
                querystring.AppendFormat("{0}={1}", p.Name.UrlEncode(), (p.Value.ToString()).UrlEncode());
            }

            return querystring.ToString();
        }
    }
}
