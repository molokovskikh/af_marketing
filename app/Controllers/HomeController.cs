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
			if (CurrentPromoter == null)
				return RedirectToAction("Login", "Account");
			return View(CurrentPromoter);
		}

		[AllowAnonymous]
		public ActionResult Error()
		{
			return View();
		}
	}
}