using System;
using System.Xml.Linq;
using System.Web.Mvc;

namespace RestSharp.TestServer
{
	public class XmlResult : ActionResult
	{
		private XDocument _doc;
		public XmlResult(XDocument doc) {
			_doc = doc;
		}

		public override void ExecuteResult(ControllerContext context) {
			context.HttpContext.Response.ContentType = "text/xml";
			_doc.Save(context.HttpContext.Response.Output);

		}
	}
}
