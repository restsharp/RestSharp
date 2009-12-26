using System;
using System.Web.Mvc;
using Grapevine.Common.Mvc;
using System.Xml.Linq;

namespace RestSharp.TestServer
{
	public abstract class ControllerBase : Controller
	{
		public new ActionResult Json(object data) {
			return new BetterJsonResult(data);
		}

		public ActionResult Xml(XDocument doc) {
			return new XmlResult(doc);
		}
	}
}
