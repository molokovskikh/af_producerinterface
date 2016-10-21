using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Tools;
using NHibernate.Linq;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.TasksManager;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
    public class AdminAccountController : BaseController
    {
        // GET: AdminAccount
        [HttpGet]
        public ActionResult Index()
        {
            var AccountModel = DB.Account.Find(CurrentUser.Id);
            return View(AccountModel);
        }

        [HttpPost]
        public ActionResult Index(ProducerInterfaceCommon.ContextModels.Account AccountModel)
        {
            var AccountBdModel = CurrentUser;

            AccountBdModel.Name = AccountModel.Name;
            AccountBdModel.Appointment = AccountModel.Appointment;

            DB.Entry(AccountBdModel).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();

            SuccessMessage("Изменения сохранены");
            return View(AccountBdModel);
        }
        public ActionResult DeleteEmail(long Id)
        {
            var DelEmailItem = DB.AccountEmail.Where(xxx => xxx.Id == Id).First();
            DB.AccountEmail.Remove(DB.AccountEmail.Where(xxx => xxx.Id == Id).First());
            DB.SaveChanges();
            return Content("Ok");
        }

        public ActionResult AddEMail(string Mail)
        {
            var NewEmail = new ProducerInterfaceCommon.ContextModels.AccountEmail();

            NewEmail.AccountId = CurrentUser.Id;
            NewEmail.eMail = Mail;

            DB.AccountEmail.Add(NewEmail);
            DB.SaveChanges();

            return PartialView(NewEmail);
        }

	    public ActionResult ServiceJobList()
	    {
			TaskManager.JobServiceUpdateServiceList(DbSession);
		    var list = DbSession.Query<ServiceTaskManager>().ToList();
		    return View(list);
	    }

	    public ActionResult ServiceJobStart(uint id,string interval = "")
	    {
		    var item = DbSession.Query<ServiceTaskManager>().FirstOrDefault(s => s.Id == id);
		    if (item == null) {
			    ErrorMessage($"Запись {id} не существует");
			    return RedirectToAction("ServiceJobList");
		    }
		    if (!item.Enabled) {
				//пока не будет реализован (перенесен) полноценный функционал для задания даты, используется JSON
			    var jDate = item.DataFromJsonGet<string>();
			    if (interval != String.Empty)
				    jDate = interval;
			    if (string.IsNullOrEmpty(jDate))
#if DEBUG
				    item.DataFromJsonSet($"0 {SystemTime.Now().AddMinutes(1).Minute} {SystemTime.Now().AddMinutes(1).Hour} * * ?");
#else
				item.DataFromJsonSet("0 0 9 1 * ?");  // раз в месяц в 9 утра
#endif
			    else
				    item.DataFromJsonSet(jDate);
					var result = new TaskManager().ServiceQuartzStart
				    (item.JobName, item.ServiceName, item.DataFromJsonGet<string>()??"");
			    if (result) {
				    item.Enabled = true;
				    item.LastModified = SystemTime.Now();
				    DbSession.Save(item);
					SuccessMessage($"Задача {id} запущена успешно");
			    } else {
				    ErrorMessage($"При запуске задачи {id} произошла ошибка");
			    }
		    } else {
			    ErrorMessage($"Задача {id} уже запущена");
			    return RedirectToAction("ServiceJobList");
		    }
		    return RedirectToAction("ServiceJobList");
	    }

	    public ActionResult ServiceJobStop(uint id)
	    {
		    var item = DbSession.Query<ServiceTaskManager>().FirstOrDefault(s => s.Id == id);
		    if (item == null) {
			    ErrorMessage($"Запись {id} не существует");
			    return RedirectToAction("ServiceJobList");
		    }
		    if (item.Enabled) {
			    var tManager = new TaskManager();
			    var result = tManager.ServiceQuartzStop(item.JobName);
			    if (result) {
				    item.Enabled = false;
				    item.LastModified = SystemTime.Now();
				    DbSession.Save(item);
				    SuccessMessage($"Задача {id} отключена успешно");
			    } else {
				    ErrorMessage($"При отключении задачи {id} произошла ошибка");
			    }
		    } else {
			    ErrorMessage($"Задача {id} уже отключена");
			    return RedirectToAction("ServiceJobList");
		    }
		    return RedirectToAction("ServiceJobList");
	    }
	}
}