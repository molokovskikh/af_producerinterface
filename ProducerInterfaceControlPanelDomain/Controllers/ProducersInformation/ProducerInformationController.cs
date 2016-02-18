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
            return View();    
        }        
    }
}