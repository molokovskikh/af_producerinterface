using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
    public class AdminAccountController : MasterBaseController
    {
        // GET: AdminAccount
        [HttpGet]
        public ActionResult Index()
        {
            var AccountModel = cntx_.Account.Find(CurrentUser.Id);           
            return View(AccountModel);
        }

        [HttpPost]
        public ActionResult Index(ProducerInterfaceCommon.ContextModels.Account AccountModel)
        {
            var AccountBdModel = CurrentUser;

            AccountBdModel.Name = AccountModel.Name;
            AccountBdModel.Appointment = AccountModel.Appointment;

            cntx_.Entry(AccountBdModel).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Изменения сохранены");
            return View(AccountBdModel);
        }
        public ActionResult DeleteEmail(long Id)
        {
            var DelEmailItem = cntx_.AccountEmail.Where(xxx => xxx.Id == Id).First();
            cntx_.AccountEmail.Remove(cntx_.AccountEmail.Where(xxx => xxx.Id == Id).First());
            cntx_.SaveChanges();
            return Content("Ok");
        }

        public ActionResult AddEMail(string Mail)
        {
            var NewEmail = new ProducerInterfaceCommon.ContextModels.AccountEmail();

            NewEmail.AccountId = CurrentUser.Id;
            NewEmail.eMail = Mail;

            cntx_.AccountEmail.Add(NewEmail);
            cntx_.SaveChanges();

            return PartialView(NewEmail);
        }
    }
}