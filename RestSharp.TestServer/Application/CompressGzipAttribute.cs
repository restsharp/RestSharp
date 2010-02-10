using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;


namespace RestSharp.TestServer
{
	public class CompressGzipAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext) {
			var response = filterContext.HttpContext.Response;

			response.AppendHeader("Content-encoding", "gzip");
			response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
		}
	}
}
