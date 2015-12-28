using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class ProfileController : pruducercontroller.BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.BreadCrumb = "Профиль пользователя";
        }

        public ActionResult Index()
        {        
            return View("Index");
        }
   

    

    }
}