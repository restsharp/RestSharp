using System;
using System.Web.Routing;

namespace RestSharp.TestServer
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start() {
			Routes.Register(RouteTable.Routes);
		}
	}
}