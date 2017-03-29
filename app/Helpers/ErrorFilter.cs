using System.Web.Mvc;
using log4net;

namespace Marketing.Helpers
{
	public class ErrorFilter : HandleErrorAttribute
	{
		public ILog log = LogManager.GetLogger(typeof(ErrorFilter));

		public override void OnException(ExceptionContext filterContext)
		{
			ThreadContext.Properties["url"] = filterContext.HttpContext.Request.Url;
			log.Error("Ошибка при выполнении запроса", filterContext.Exception);
			base.OnException(filterContext);
		}
	}
}