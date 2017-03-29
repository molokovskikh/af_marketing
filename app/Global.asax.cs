using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using log4net.Config;
using Marketing.Controllers;
using Marketing.Helpers;

namespace Marketing
{
	public class MvcApplication : HttpApplication
	{
		static ILog Log = LogManager.GetLogger(typeof(MvcApplication));

		protected void Application_Start()
		{
			try {
				XmlConfigurator.Configure();
				GlobalContext.Properties["version"] = typeof(HomeController).Assembly.GetName().Version;
				var hibernate = new NHibernate();
				hibernate.Init();
				GlobalFilters.Filters.Add(new ErrorFilter());
				GlobalFilters.Filters.Add(new SessionFilter(hibernate.Factory));

				AreaRegistration.RegisterAllAreas();
				RouteConfig.RegisterRoutes(RouteTable.Routes);
				BundleConfig.RegisterBundles(BundleTable.Bundles);
			} catch (Exception e) {
				Log.Error("Ошибка при инициализации приложения", e);
				throw;
			}
		}
	}
}
