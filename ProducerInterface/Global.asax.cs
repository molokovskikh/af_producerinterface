using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using ProducerInterfaceCommon.Heap;
using System.Collections.Generic;
using log4net.Config;

namespace ProducerInterface
{
	// using Common.Logging;
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			ViewEngines.Engines.Add(new MyViewEngine());
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			XmlConfigurator.Configure();
		}


		protected void Application_Error(object sender, EventArgs e)
		{
			if (Server != null)
			{
				var ex = Server.GetLastError();
				ILog logger = LogManager.GetLogger(GetType());

				// временно. TODO наладить log4net
				var sb = new StringBuilder();
				sb.AppendLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")} ERROR");
				sb.AppendLine($"{ThreadContext.Properties["user"]} - {ThreadContext.Properties["url"]}");
				sb.AppendLine($"{ex.GetType()}: {ex.Message}");
				sb.AppendLine(ex.StackTrace);
				var to = new List<string>() { "137@analit.net" };
				EmailSender.SendEmail(to, "Ошибка в клиентской части Интерфейса производителя", sb.ToString(), null);

				logger.Error(ex.Message, ex);

				if (Response.StatusCode != 404)
				{

				}
				ErrorMessage("При выполнении запроса произошла непредвиденная ошибка");

				Response.Redirect("~/Home/Index");
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

