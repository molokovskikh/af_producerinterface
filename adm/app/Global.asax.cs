using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using log4net.Config;
using log4net.ObjectRenderer;
using MySql.Data.MySqlClient;

namespace ProducerInterfaceControlPanelDomain
{
		public class ExceptionRenderer : IObjectRenderer
	{
		public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
		{
			var ex = obj as DbEntityValidationException;
			if (ex != null) {
				writer.Write(ex);
				writer.WriteLine();
				foreach (var childEx in ex.EntityValidationErrors) {
					writer.Write(childEx);
					writer.WriteLine();
				}
			}
		}
	}

	public class MvcApplication : HttpApplication
	{
		static ILog Log = LogManager.GetLogger(typeof(MvcApplication));

		protected void Application_Start()
		{
			try {
				ViewEngines.Engines.Add(new ProducerInterfaceCommon.Heap.MyViewEngine());
				AreaRegistration.RegisterAllAreas();
				FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
				RouteConfig.RegisterRoutes(RouteTable.Routes);
				BundleConfig.RegisterBundles(BundleTable.Bundles);
				XmlConfigurator.Configure();
				Log.Logger.Repository.RendererMap.Put(typeof(Exception), new ExceptionRenderer());
			} catch(Exception e) {
				Log.Error("Ошибка при инициализации приложения", e);
				throw;
			}
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			if (HttpContext.Current.IsCustomErrorEnabled)
			{
				var ex = Server.GetLastError();
				Log.Error(ex.Message, ex);
				ErrorMessage("При выполнении запроса произошла непредвиденная ошибка");
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
