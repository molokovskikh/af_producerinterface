using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    }
}