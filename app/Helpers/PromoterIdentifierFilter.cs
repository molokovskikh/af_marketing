using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using Marketing.Controllers;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;

namespace Marketing.Helpers
{
	public class PromoterIdentifierFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var dbSession = (ISession) context.HttpContext.Items[typeof (ISession)];
			context.HttpContext.Items[typeof (Promoter)] = null;
			if (context.HttpContext.User?.Identity?.Name != null) {
				var promoter = dbSession.Query<Promoter>()
					.FirstOrDefault(r => r.Login == context.HttpContext.User.Identity.Name.Replace(Promoter.ACC_LOGIN_PREFIX, ""));
				if (promoter != null && context.HttpContext.User.Identity.Name.IndexOf(Promoter.ACC_LOGIN_PREFIX) != -1) {
					context.HttpContext.Items[typeof (Promoter)] = promoter;
					context.Controller.ViewBag.CurrentPromoter = promoter;
				} else {
					var controller = context.Controller as BaseController;
					var isNotAccountController = !(context.Controller is AccountController);
					if (controller != null && isNotAccountController) {
						context.Result = controller.BaseRedirectToAction("Register", "Account");
					} else {
						//для авторизованного пользователя, идущего в AccountController
						context.Controller.ViewBag.CurrentPromoter = new Promoter {Name = "Администратор"};
					}
				}
			}
		}
	}
}