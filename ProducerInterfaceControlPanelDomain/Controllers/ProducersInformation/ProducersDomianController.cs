using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.ProducersInformation
{
    public class ProducersDomainController : MasterBaseController
    {
        // GET: ProducersDomian
        [HttpGet]
        public ActionResult Index()
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

            ViewBag.ProducerList = h.RegisterListProducer();

            return View();
        }
        [HttpPost]
        public ActionResult GetDomain(long idproducer)
        {    
            var ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.ProducerId == idproducer).ToList();
            return PartialView(ListDomain);
        }

        [HttpPost]
        public ActionResult AddDomain(long AccountCompanyId, string NewDomainName)
        {
            var CompanyDomainNameNew = new ProducerInterfaceCommon.ContextModels.CompanyDomainName();
            CompanyDomainNameNew.CompanyId = AccountCompanyId;
            CompanyDomainNameNew.Name = NewDomainName;

            cntx_.CompanyDomainName.Add(CompanyDomainNameNew);
            cntx_.SaveChanges();

            var ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.Id == AccountCompanyId).ToList();
            return PartialView("GetDomain",ListDomain);
        }

        [HttpPost]
        public ActionResult DeleteDomain(long Id)
        {           
            var CompanyDomainNameDel = cntx_.CompanyDomainName.Find(Id);
            var CompanyId = CompanyDomainNameDel.AccountCompany.Id;

            var AccountBAN = cntx_.Account.Where(xxx => xxx.Login.Contains(CompanyDomainNameDel.Name) && xxx.CompanyId == CompanyId).ToList();
            foreach (var AccountItem in AccountBAN)
            {
                AccountItem.Enabled = 0;
                AccountItem.PasswordUpdated = DateTime.Now;
                cntx_.Entry(AccountItem).State = System.Data.Entity.EntityState.Modified;
            }

            cntx_.CompanyDomainName.Remove(CompanyDomainNameDel);
            cntx_.SaveChanges();                      

            ViewBag.Delete = CompanyDomainNameDel.Name;

            var ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.Id == CompanyId).ToList();
            return PartialView("GetDomain", ListDomain);
        }


        public ActionResult AppointmentNameList()
        {
            ViewBag.EnabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 1).ToList();
            ViewBag.DisabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 0).ToList();
            ViewBag.AccountList = cntx_.Account.Where(xxx => xxx.TypeUser == 0 && xxx.Enabled== 1).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AppointmentEnable(int Id, int enabled)
        {
            var AppointmentDeactivate = cntx_.AccountAppointment.Find(Id);
            AppointmentDeactivate.GlobalEnabled =(sbyte) enabled;
            cntx_.Entry(AppointmentDeactivate).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            ViewBag.EnabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 1).ToList();
            ViewBag.DisabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 0).ToList();
            ViewBag.AccountList = cntx_.Account.Where(xxx => xxx.TypeUser == 0 && xxx.Enabled == 1).ToList();

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
            var NewAppointment = new ProducerInterfaceCommon.ContextModels.AccountAppointment();
            NewAppointment.Name = NewAppointmentname;
            NewAppointment.GlobalEnabled = 1;

            cntx_.Entry(NewAppointment).State = System.Data.Entity.EntityState.Added;

            cntx_.SaveChanges();

            ViewBag.AccountList = cntx_.Account.Where(xxx => xxx.TypeUser == 0 && xxx.Enabled == 1).ToList();
            ViewBag.EnabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 1).ToList();
            ViewBag.DisabledList = cntx_.AccountAppointment.Where(xxx => xxx.GlobalEnabled == 0).ToList();

            ViewBag.Results = "Добавлена в список новая должность '" + NewAppointmentname + "'";

            return PartialView("partial/appointment");
        }

    }
}