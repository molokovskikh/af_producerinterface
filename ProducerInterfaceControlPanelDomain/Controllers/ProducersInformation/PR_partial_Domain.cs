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

            var DomainProduceList = cntx_.CompanyDomainName.Where(x => x.CompanyId == AccountCompanyId).ToList().Where(x=>x.Name == NewDomainName).ToList();
            var ListDomain = new List<ProducerInterfaceCommon.ContextModels.CompanyDomainName>();

            if (DomainProduceList.Count() >= 1)
            {
                ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.Id == AccountCompanyId).ToList();             
                ViewBag.ErrorAdd = "Данный домен уже есть в списке данного производителя";
                return PartialView("GetDomain", ListDomain);
            }
            
            var CompanyDomainNameNew = new ProducerInterfaceCommon.ContextModels.CompanyDomainName();
            CompanyDomainNameNew.CompanyId = AccountCompanyId;
            CompanyDomainNameNew.Name = NewDomainName;

            cntx_.CompanyDomainName.Add(CompanyDomainNameNew);
            cntx_.SaveChanges();

            ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.Id == AccountCompanyId).ToList();
            return PartialView("GetDomain",ListDomain);
        }

        public ActionResult DeleteDomain(long Id)
        {           
            var CompanyDomainNameDel = cntx_.CompanyDomainName.Find(Id);
            var CompanyId = CompanyDomainNameDel.AccountCompany.Id;
                    
            var DomainList = cntx_.CompanyDomainName.Where(x => x.CompanyId == CompanyId).ToList();

            if (DomainList.Count() != 1)
            {
                cntx_.CompanyDomainName.Remove(CompanyDomainNameDel);
                cntx_.SaveChanges();
                ViewBag.Delete = CompanyDomainNameDel.Name;
            }
            else
            {
                ViewBag.Delete = CompanyDomainNameDel.Name + " не ";
            }           

            var ListDomain = cntx_.CompanyDomainName.Where(xxx => xxx.AccountCompany.Id == CompanyId).ToList();
            return PartialView("GetDomain", ListDomain);
        }

    }
}