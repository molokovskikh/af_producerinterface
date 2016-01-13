using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base

        public Models.producerinterface_Entities cntx_ = new Models.producerinterface_Entities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.currentUser = User.Identity.Name;
        }

        public string GetWebConfigParameters(string ParamKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[ParamKey].ToString();
        }
        public string DebugShedulerName()
        {
            string Name = "ServerScheduler";
#if DEBUG
            Name = "TestScheduler";
#endif
            return Name;
        }
    }

}