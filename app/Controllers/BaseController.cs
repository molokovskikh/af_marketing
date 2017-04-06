using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;

namespace Marketing.Controllers
{
	public class BaseController : Controller
	{
		protected ISession DbSession => HttpContext.Items[typeof (ISession)] as ISession;
		public Promoter CurrentPromoter => HttpContext.Items[typeof (Promoter)] as Promoter;

		public void SuccessMessage(string message)
		{
			TempData["SuccessMessage"] = message;
		}

		public void ErrorMessage(string message)
		{
			TempData["ErrorMessage"] = message;
		}

		public RedirectToRouteResult BaseRedirectToAction(string action, string controller)
		{
			return RedirectToAction(action, controller);
		}
	}
}