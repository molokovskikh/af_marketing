using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marketing.Controllers
{
	public class ReportController : BaseController
	{
		public ActionResult Members()
		{
			return View();
		}

		public ActionResult MarketingEvents()
		{
			return View();
		}
	}
}