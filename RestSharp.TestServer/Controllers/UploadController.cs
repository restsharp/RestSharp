using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace RestSharp.TestServer.Controllers
{
    public class UploadController : Controller
    {
        public string Index(string foo)
        {
			foreach (string file in Request.Files) {
				var hpf = Request.Files[file] as HttpPostedFileBase;
				hpf.SaveAs(Server.MapPath("~/Upload/" + hpf.FileName));
			}

			return foo;
        }
    }
}
