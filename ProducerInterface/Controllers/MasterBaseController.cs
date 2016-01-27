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
            TypeLoginUser = ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser;          
            base.OnActionExecuting(filterContext);

        }








    }
}