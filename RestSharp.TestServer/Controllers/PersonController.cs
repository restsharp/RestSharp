using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace RestSharp.TestServer.Controllers
{
    public class PersonController : ControllerBase
    {
        public ActionResult Index()
        {
			return Json(new { });
        }
    }
}
