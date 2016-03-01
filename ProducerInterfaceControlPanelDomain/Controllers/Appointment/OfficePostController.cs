using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.ProducersInformation
{
    public class OfficePostController : MasterBaseController
    {
        // GET: OfficePost
        public ActionResult Index()
        {
            var AppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();
            return View(AppointmentList);          
        }

        public ActionResult GlobalAppointmentActivate(int IdAppointment, bool Active)
        {
            var ChangeAppointment = cntx_.AccountAppointment.Find(IdAppointment);
            if (Active)
            {
                ChangeAppointment.GlobalEnabled = 1;
            }
            else
            {
                ChangeAppointment.GlobalEnabled = 2;
            }
            cntx_.Entry(ChangeAppointment).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            var AppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();
            return PartialView("ListAppointment", AppointmentList);
        }

        public ActionResult GlobalAppointmentChange(int idAppointment, string nameAppointmentName)
        {
            var AppointmentChange = cntx_.AccountAppointment.Find(idAppointment);
            AppointmentChange.Name = nameAppointmentName;
            cntx_.SaveChanges();
            var AppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();
            return PartialView("ListAppointment", AppointmentList);
        }

        // на вход приходит id удаляемой должности и id должности которую установить для пользователей которые данную должность выбрали
        public ActionResult GlobalAppointmentDelete(int idDelAppointment)
        {

            var AccountList = cntx_.Account.Where(x => x.AppointmentId == idDelAppointment).ToList();

            foreach (var AccountItem in AccountList)
            {
                AccountItem.AppointmentId = null;
                cntx_.Entry(AccountItem).State = System.Data.Entity.EntityState.Modified;
            }
            cntx_.SaveChanges();

            cntx_.AccountAppointment.Remove(cntx_.AccountAppointment.Find(idDelAppointment));
            cntx_.SaveChanges(CurrentUser, "Удаление должности из глобального списка");

            var AppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();
            return PartialView("ListAppointment", AppointmentList);
        }

        public ActionResult GlobalAppointmentAdd(string NewNameAppointment)
        {
            var NewAppointment = new AccountAppointment();
            NewAppointment.Name = NewNameAppointment;
            NewAppointment.GlobalEnabled = 1;

            cntx_.AccountAppointment.Add(NewAppointment);
            cntx_.SaveChanges();
            var AppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();
            return PartialView("ListAppointment", AppointmentList);
        }

        /* Управление спиком должностей для конкретного пользователя */

        public ActionResult CustomAppointmentList()
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();
            return View("CustomAppointment");
        }

        public ActionResult GetAccountInProducer(long IdProducer)
        {
            var AccountListModel = cntx_.Account.Where(x => x.AccountCompany != null).ToList().Where(x => x.AccountCompany.ProducerId == IdProducer).ToList();
            return PartialView("Partial/ListAccount", AccountListModel);
        }

        public ActionResult GetListAccountAppointment(long AccountId)
        {
            ViewBag.AccountId = AccountId;
            var AccountAppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == AccountId).ToList();
            return PartialView("Partial/ListAccountAppointment", AccountAppointmentList);
        }

        public ActionResult AddCustomAppointment(long AccountId, string NewPostName)
        {

            var NewAppointment = new AccountAppointment();
            NewAppointment.Name = NewPostName;
            NewAppointment.IdAccount = AccountId;

            cntx_.AccountAppointment.Add(NewAppointment);
            cntx_.SaveChanges();

            ViewBag.AccountId = AccountId;

            var AccountAppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == AccountId).ToList();
            return PartialView("Partial/ListAccountAppointment", AccountAppointmentList);
        }

        public ActionResult DeleteCustomAppointment(int idDelAppointment, int AccountId)
        {
            var Del = cntx_.AccountAppointment.Find(idDelAppointment);
            var AccountList = cntx_.Account.Where(x => x.AppointmentId == Del.Id).ToList();

            foreach (var AccountItem in AccountList)
            {
                AccountItem.AppointmentId = null;
                cntx_.Entry(AccountItem).State = System.Data.Entity.EntityState.Modified;
            }

            cntx_.SaveChanges();

            cntx_.Entry(Del).State = System.Data.Entity.EntityState.Deleted;

            cntx_.SaveChanges();


            ViewBag.AccountId = AccountId;
            var AccountAppointmentList = cntx_.AccountAppointment.Where(x => x.IdAccount == AccountId).ToList();
            return PartialView("Partial/ListAccountAppointment", AccountAppointmentList);

        }

        [HttpGet]
        public ActionResult AccountNotPost()
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();

            ViewBag.AccountAppointmentGlobalList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();

            var AccountListModel = cntx_.Account.Where(x => x.AppointmentId == null && x.TypeUser == (sbyte) TypeUsers.ProducerUser).ToList();
            return View(AccountListModel);
        }
        [HttpPost]
        public ActionResult AccountNotPost(long AccountId, int IdPost)
        {

            if (IdPost != 0)
            {
                var AccountChange = cntx_.Account.Find(AccountId);
                AccountChange.AppointmentId = IdPost;

                cntx_.Entry(AccountChange).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges();
                SuccessMessage("Должность добавлена пользователю " + AccountChange.Login + " " + AccountChange.Name);
            }
            else
            {
                ErrorMessage("Не выбрана должность");
            }
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();

            ViewBag.AccountAppointmentGlobalList = cntx_.AccountAppointment.Where(x => x.IdAccount == null).ToList();

            var AccountListModel = cntx_.Account.Where(x => x.AppointmentId == null && x.TypeUser == (sbyte)TypeUsers.ProducerUser).ToList();
            return View(AccountListModel);
        }


    }
}