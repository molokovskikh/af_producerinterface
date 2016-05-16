using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface
{
	// using Common.Logging;
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			ViewEngines.Engines.Add(new ProducerInterfaceCommon.Heap.MyViewEngine());
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}


		protected void Application_Error(object sender, EventArgs e)
		{
			if (Server != null)
			{
				Exception ex = Server.GetLastError();
				ILog _logger = LogManager.GetLogger("MySqlAdoNetAppender");

				// временно. TODO наладить log4net
				EmailSender.SendEmail("service@analit.net", "Ошибка в Интерфейсе производителя", ex.Message, null, false);

				_logger.Error(ex.Message, ex);

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

