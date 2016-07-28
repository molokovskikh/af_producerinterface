using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using ProducerInterfaceCommon.Heap;
using log4net.Config;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterface
{
	public class MvcApplication : HttpApplication
	{
		static ILog Log = LogManager.GetLogger(typeof(MvcApplication));

		protected void Application_Start()
		{
			XmlConfigurator.Configure();
			ViewEngines.Engines.Add(new MyViewEngine());
			AreaRegistration.RegisterAllAreas();

			var nh = new ProducerInterfaceCommon.ContextModels.NHibernate();
			nh.Init();
			GlobalFilters.Filters.Add(new ErrorFilter());
			GlobalFilters.Filters.Add(new SessionFilter(nh.Factory));

			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			Log.Logger.Repository.RendererMap.Put(typeof(Exception), new ExceptionRenderer());
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			var ex = Server.GetLastError();
			Log.Error(ex.Message, ex);
		}
	}
}

