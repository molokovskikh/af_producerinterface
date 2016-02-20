using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public partial class ProducerInformationController : MasterBaseController
    {
        // GET: ProducersDomian
        [HttpGet]
        public ActionResult DomainList()
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

    }
}