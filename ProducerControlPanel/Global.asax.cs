using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AnalitFramefork;

namespace ProducerControlPanel
{
	/// <summary>
	/// Главный файл проекта MVC
	/// </summary>
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			log4net.Config.XmlConfigurator.Configure();
			Framework.Initialize(this);
		}
	}
}