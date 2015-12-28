using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class HomeController : pruducercontroller.BaseController
    {
        public ActionResult Index()
        {        
            return View("Index");
        }    
    }
}