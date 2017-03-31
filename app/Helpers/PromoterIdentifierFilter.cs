using System;
using System.Linq;
using System.Web.Mvc;
using log4net;
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
			context.HttpContext.Items[typeof (Promoter)] = dbSession.Query<Promoter>().FirstOrDefault(); //нужно сгененрировать тестовые данные

		}
	}
}