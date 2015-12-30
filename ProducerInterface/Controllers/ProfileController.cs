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
        protected Quartz.IScheduler scheduler;

        protected IScheduler GetRemoteSheduler()
        {
            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            var sf = new StdSchedulerFactory(props);
            var scheduler = sf.GetScheduler();

            // проверяем имя шедулера
            if (scheduler.SchedulerName != "ServerScheduler")
                throw new NotSupportedException("Должен использоваться ServerScheduler");

            // проверяем удалённый ли шедулер
            var metaData = scheduler.GetMetaData();
            if (!metaData.SchedulerRemote)
                throw new NotSupportedException("Должен использоваться удалённый ServerScheduler");

            return scheduler;
        }

        public ActionResult Index()
        {
            var ModelView = new Models.RegistrerValidation();
            Quartz.Job.EDM.reportData cntx;
            Quartz.Job.NamesHelper h;
            cntx = new Quartz.Job.EDM.reportData();
            h = new Quartz.Job.NamesHelper(cntx, 0);
            scheduler = GetRemoteSheduler();

            var jobList = cntx.jobextend.Where(x => x.ProducerId == CurrentUser.ProducerId
                                                                                        && x.SchedName == scheduler.SchedulerName
                                                                                        && x.Enable == true).ToList();

            return View(jobList);

        }

        public ActionResult Promotions()
        {
            var currentUser = GetCurrentUser();
            var list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId && xxx.Status).ToList();
            ViewBag.PromotionList = list;
            return View();
        }










    }
}