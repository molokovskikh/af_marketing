using System;
using System.Web.Mvc;

namespace Marketing.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}