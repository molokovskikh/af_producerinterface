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

            ViewBag.Producernames = cntx_.producernames.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().ProducerName;
            // в разработке
            //var AC = new ProducerInterfaceCommon.Controllers.Account_Statistics(HttpContext, CurrentUser);
            
            //new Thread(() =>
            //{
            //    AC.SaveAccountAsync();              
            //}).Start();               
                
        }   
    }
}