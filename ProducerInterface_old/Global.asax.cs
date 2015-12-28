using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AnalitFramefork;
using System.Web.Optimization;
using System.Collections.Specialized;
using System.Configuration;
using Quartz.Impl;

namespace ProducerInterface
{
	/// <summary>
	/// Главный файл проекта MVC
	/// </summary>
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{        
                AreaRegistration.RegisterAllAreas();
                WebApiConfig.Register(GlobalConfiguration.Configuration);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                ProducerInterface.BundleConfig.RegisterBundles(BundleTable.Bundles);
                Framework.Initialize(this);         
		}

        protected void Application_End()
        {
#if DEBUG
            //var props = (NameValueCollection)ConfigurationManager.GetSection("quartzDebug");
            //var sf = new StdSchedulerFactory(props);
            //var scheduler = sf.GetScheduler();
            //if (scheduler.IsStarted)
            //    scheduler.Shutdown();
#endif
        }
    }
}