using System;
using System.Web.Mvc;
using NHibernate;

namespace Marketing.Controllers
{
#if !DEBUG
	[Authorize]
#endif
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			if (System.Web.HttpContext.Current.Session["promoter"] == null)
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