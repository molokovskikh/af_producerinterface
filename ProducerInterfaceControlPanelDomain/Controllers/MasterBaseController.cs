using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class MasterBaseController : ProducerInterfaceCommon.Controllers.BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            CurrentUser = GetCurrentUser(ProducerInterfaceCommon.ContextModels.TypeUsers.ControlPanelUser);



        }
















    }
}