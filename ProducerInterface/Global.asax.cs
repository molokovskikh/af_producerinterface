using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;

using log4net.Core;

namespace ProducerInterface
{
    // using Common.Logging;
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
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

                log4net.Config.XmlConfigurator.Configure();

                ILog _logger = LogManager.GetLogger("MySqlAdoNetAppender");

                _logger.Error(ex.Message.ToString());

                if (Response.StatusCode != 404)
                {

                }
                ErrorMessage("При выполнении запроса произошла непредвиденная ошибка");

              //  Response.Redirect("~/Home/Index");
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

