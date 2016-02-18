using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{   
    public class ProducerInformationController : MasterBaseController
    {        
        [HttpGet]       
        public ActionResult Index()
        {
            // ListProducers - список не только компаниий производителей но и компаний с анонимным производителем 
            // точнее с анонимными пользователями.

            var ListProducers = cntx_.AccountCompany.ToList();
            return View(ListProducers);    
        }        
    }
}