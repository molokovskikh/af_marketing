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
			return View(CurrentPromoter);
		}
	}
}