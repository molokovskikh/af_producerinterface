using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;

namespace ProducerInterface.Controllers
{
    public class MasterBaseController : ProducerInterfaceCommon.Controllers.BaseController
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            TypeLoginUser = ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser;          
            base.OnActionExecuting(filterContext);

            if (CurrentUser != null)
            {
                if (CurrentUser.AccountCompany.ProducerId != null)
                {
                    ViewBag.Producernames = cntx_.producernames.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().ProducerName;
                }
                else
                {
                    ViewBag.Producernames = "Физическое лицо";
                }
            }
                 
        }   
    }
}