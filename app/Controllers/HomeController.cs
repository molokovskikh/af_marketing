using System;
using System.Web.Mvc;
using NHibernate;

namespace Marketing.Controllers
{
	public class HomeController : Controller
	{
		protected ISession DbSession => HttpContext.Items[typeof(ISession)] as ISession;

		public ActionResult Index()
		{
			return View();
		}
	}
}