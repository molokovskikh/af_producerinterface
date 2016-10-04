using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.Helpers;
using ProducerInterfaceControlPanelDomain.Controllers;

namespace ProducerInterfaceControlPanelDomain
{
	public class MvcApplication : HttpApplication
	{
		static ILog Log = LogManager.GetLogger(typeof(MvcApplication));

		protected void Application_Start()
		{
			try {
				XmlConfigurator.Configure();
				Log.Logger.Repository.RendererMap.Put(typeof(Exception), new ExceptionRenderer());
				GlobalContext.Properties["version"] = typeof(SlideController).Assembly.GetName().Version;
				ViewEngines.Engines.Add(new ProducerInterfaceCommon.Heap.MyViewEngine());
				AreaRegistration.RegisterAllAreas();

				var nh = new ProducerInterfaceCommon.ContextModels.NHibernate();
				nh.Init();
				GlobalFilters.Filters.Add(new ErrorFilter());
				GlobalFilters.Filters.Add(new SessionFilter(nh.Factory));

				RouteConfig.RegisterRoutes(RouteTable.Routes);
				BundleConfig.RegisterBundles(BundleTable.Bundles);
			} catch(Exception e) {
				Log.Error("Ошибка при инициализации приложения", e);
				throw;
			}
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			try {
				ThreadContext.Properties["url"] = Request.Url;
			} catch {
			}
			var ex = Server.GetLastError();
			Log.Error(ex.Message, ex);
		}
	}
}
