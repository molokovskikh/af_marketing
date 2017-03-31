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
			var promoter = System.Web.HttpContext.Current.Session["promoter"] as Promoter;
			context.HttpContext.Items[typeof(Promoter)] = promoter == null
				? dbSession.Query<Promoter>().FirstOrDefault() //нужно сгененрировать тестовые данные
				: dbSession.Query<Promoter>().FirstOrDefault(r => r.Id == promoter.Id);
		}
	}
}