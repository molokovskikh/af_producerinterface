using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using Quartz.Job.EDM;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using Quartz.Job.Models;
using System.Configuration;
using System.Collections.Specialized;

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
            return View();
        }

    }
}