using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class MasterBaseController : ProducerInterfaceCommon.Controllers.BaseController
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            CurrentUser = GetCurrentUser(ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser);

        }

        // GET: MasterBase
        public ActionResult Index()
        {
            return View();
        }








    }
}