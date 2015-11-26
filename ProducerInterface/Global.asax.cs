using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AnalitFramefork;
using System.Web.Optimization;
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
	}
}