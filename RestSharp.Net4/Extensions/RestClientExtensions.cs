using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public static partial class RestClientExtensions
	{
		public static RestResponse<dynamic> ExecuteDynamic(this IRestClient client, IRestRequest request)
		{
			var response = client.Execute(request);

			var generic = (RestResponse<dynamic>)response;
			dynamic content = SimpleJson.DeserializeObject(response.Content);
			generic.Data = content;

			return generic;
		}
	}
}
