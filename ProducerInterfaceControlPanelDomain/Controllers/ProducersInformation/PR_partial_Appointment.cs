using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public partial class ProducerInformationController : MasterBaseController
    {

        public ActionResult PostnameList()
        {
            return View();

        }

        public ActionResult PostNameList(long AccountId)
        {
            return View();
        }

        public ActionResult GetAccountAppointment(long AccountId)
        {
            var PostModel = cntx_.AccountAppointment.Where(x => x.IdAccount == AccountId).ToList();
            return PartialView(PostModel);
        }


        public ActionResult GetAccountList(long ProducerId)
        {
            var AccountListModel = cntx_.Account.Where(x => x.AccountCompany != null).ToList().Where(x => x.AccountCompany.ProducerId == ProducerId).ToList();
            return PartialView(AccountListModel);
        }


        public ActionResult AddAppointmentToUser(long IdAccount, string NewNameAppointment= null, int IdAppointment = 0, bool AddOrNot = true)
        {

            if (NewNameAppointment == null && IdAppointment == 0)
            {
                var ListReturnAppointment = cntx_.AccountAppointment.Where(x => x.IdAccount == IdAccount).ToList();
            }

            if (AddOrNot)
            {
                var NewAppointment = new ProducerInterfaceCommon.ContextModels.AccountAppointment();

                NewAppointment.Name = NewNameAppointment;
                NewAppointment.IdAccount = IdAccount;

                cntx_.AccountAppointment.Add(NewAppointment);
                cntx_.SaveChanges();
                return PartialView("Partial/ListAppointmentsToOneAccount");
            }
            else
            {
                return PartialView("Partial/ListAppointmentsToOneAccount");
            }

        }

       

        public ActionResult AppointmentNameList()
        {
            ListViewData();
            return View();
        }

        [HttpPost]
        public ActionResult AppointmentEnable(int Id, int enabled)
        {
            var AppointmentDeactivate = cntx_.AccountAppointment.Find(Id);
            AppointmentDeactivate.GlobalEnabled = (sbyte)enabled;
            cntx_.Entry(AppointmentDeactivate).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();
                 
            if (enabled == 0)
            {
                ViewBag.Results = AppointmentDeactivate.Name + " Добавлена в список";
            }
            else
            {
                ViewBag.Results = AppointmentDeactivate.Name + " Удалена из списока";
            }

            return PartialView("partial/appointment");
        }

        public ActionResult AddNewAppointment(string NewAppointmentname)
        {
            ListViewData();
            var NewAppointment = new ProducerInterfaceCommon.ContextModels.AccountAppointment();
            NewAppointment.Name = NewAppointmentname;
            NewAppointment.GlobalEnabled = 1;

            cntx_.Entry(NewAppointment).State = System.Data.Entity.EntityState.Added;

            cntx_.SaveChanges();      

            ViewBag.Results = "Добавлена в список новая должность '" + NewAppointmentname + "'";

            return PartialView("partial/appointment");
        }
        
        public ActionResult GetAppointmentProducer(int Id)
        {
            ListViewData(true, Id);
      
            return PartialView("producerappointment");
        }

        public ActionResult AddProducerAppointment(string Name, int Id)
        {

            var NewAppointmentToProducer = new ProducerInterfaceCommon.ContextModels.AccountAppointment();

            NewAppointmentToProducer.Name = Name;
            NewAppointmentToProducer.IdAccount = Id;
            NewAppointmentToProducer.GlobalEnabled = 0;
            cntx_.AccountAppointment.Add(NewAppointmentToProducer);
            cntx_.SaveChanges();

            ViewBag.Messages = "Должность добавлена";
            ListViewData(true, Id);
            return PartialView("producerappointment");
        }

        public ActionResult DeleteProducerAppointment(int Id, int IdCompany)
        {
            var NewAppointmentToProducer = cntx_.AccountAppointment.Find(Id);
         
            cntx_.AccountAppointment.Remove(NewAppointmentToProducer);
            cntx_.SaveChanges();

            ViewBag.Messages = "Должность удалена";
            ListViewData(true, IdCompany);
            return PartialView("producerappointment");
        }
        
        private void ListViewData(bool ProducerAppointment = false, int AccountCompanyId =0)
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

            if (ProducerAppointment)
            {
                ViewBag.AccountCompanyId = AccountCompanyId;
                ViewBag.AccountCompanyname = h.RegisterListProducer().ToList().Where(xxx => xxx.Value == cntx_.AccountCompany.Where(z=>z.Id == AccountCompanyId).First().ProducerId.ToString()).First().Text;
                ViewBag.EnabledListProducer = cntx_.AccountAppointment.Where(xxx =>xxx.IdAccount== AccountCompanyId).ToList();            
                ViewBag.AccountList = cntx_.Account.Where(xxx => xxx.TypeUser == 0 && xxx.Enabled == 1 && xxx.AccountCompany.Id == AccountCompanyId).ToList();
            }
            else
            {
                ViewBag.EnabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 1).ToList();
                ViewBag.DisabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 0 && xxx.IdAccount == null).ToList();
                ViewBag.AccountList = cntx_.Account.Where(xxx => xxx.TypeUser == (sbyte) ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser && xxx.Enabled == 1).ToList();
                ViewBag.ProducerList = cntx_.AccountCompany.ToList();
            
                var ProducerNames = h.RegisterListProducer();
                ProducerNames.Add(new ProducerInterfaceCommon.ContextModels.OptionElement() { Value = "", Text = "" });
                ProducerNames.Reverse();
                ViewBag.ProducerNames = ProducerNames;
            }
        }

    }
}