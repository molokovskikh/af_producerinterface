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
			if (HttpContext.Current.IsCustomErrorEnabled) {
				var ex = Server.GetLastError();
				Log.Error(ex.Message, ex);
				ErrorMessage("При выполнении запроса произошла непредвиденная ошибка");
				Response.Redirect("~");
			}
		}

		public void ErrorMessage(string message)
		{
			SetCookie("ErrorMessage", message);
		}

		public void SetCookie(string name, string value)
		{
			if (value == null)
			{
				Response.Cookies.Add(new HttpCookie(name, "false") { Path = "/", Expires = DateTime.Now });
				return;
			}
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
			var text = Convert.ToBase64String(plainTextBytes);
			Response.Cookies.Add(new HttpCookie(name, text) { Path = "/" });
		}

	}
}

