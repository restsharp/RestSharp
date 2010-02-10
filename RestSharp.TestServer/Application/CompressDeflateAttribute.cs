using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;

namespace RestSharp.TestServer
{
	public class CompressDeflateAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext) {
			var response = filterContext.HttpContext.Response;

			response.AppendHeader("Content-encoding", "deflate");
			response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
		}
	}
}