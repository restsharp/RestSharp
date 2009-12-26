using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestSharp.TestServer
{
	public class Routes
	{
		public static void Register(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute("Default", "{controller}/{action}",
				new { controller = "Home", action = "Index" } 
			);
		}
	}
}
