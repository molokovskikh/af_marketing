using System;
using System.Web.Mvc;
using NHibernate;

namespace Marketing.Controllers
{
	[Authorize]
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}