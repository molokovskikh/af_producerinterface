using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using Quartz;
using Quartz.Impl;
using ProducerInterfaceCommon.Heap;
using System.Configuration;
using System.Collections.Specialized;

namespace ProducerInterface.Controllers
{
    public class ProfileController : MasterBaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.BreadCrumb = "Профиль пользователя";            
        }
    
        public ActionResult Index()
        {   
            return View();
        }

        public ActionResult Account()
        {
            SuccessMessage("В разработке");
            return RedirectToAction("Index");
        }
        

    }
}