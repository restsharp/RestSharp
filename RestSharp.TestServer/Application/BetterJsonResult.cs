using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Grapevine.Common.Mvc
{
	public class BetterJsonResult : ActionResult
	{
		object _data;
		public BetterJsonResult(object data) {
			_data = data;
		}

		public override void ExecuteResult(ControllerContext context) {
			var json = JsonConvert.SerializeObject(_data, Formatting.None, new IsoDateTimeConverter());
			context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
			context.HttpContext.Response.Write(json);
		}
	}
}
