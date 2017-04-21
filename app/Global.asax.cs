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
				GlobalFilters.Filters.Add(new ErrorFilter(),0);
				GlobalFilters.Filters.Add(new SessionFilter(hibernate.Factory),1);
				GlobalFilters.Filters.Add(new PromoterIdentifierFilter(), 2);

				AreaRegistration.RegisterAllAreas();
				RouteConfig.RegisterRoutes(RouteTable.Routes);
				BundleConfig.RegisterBundles(BundleTable.Bundles);

				ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();
				Log.Info("Приложение успешно запущено");
			} catch (Exception e) {
				Log.Error("Ошибка при инициализации приложения", e);
				throw;
			}
		}

		public void Application_Error(object sender, EventArgs e)
		{
			Log.Error("Ошибка при обработке запроса", Server.GetLastError());
		}
	}
}
