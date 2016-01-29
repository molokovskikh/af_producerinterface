using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using System.Collections;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class HomeController : MasterBaseController
    {
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Стартовая страница сайта, которая будет доступна после авторизации</returns>
        public ActionResult Index()
        {
            //IEnumerable<string> X = Groups().ToList();
            //ViewBag.ListGroup = X;

            var ListActionInProducers = cntx_.promotions.Where(xxx => xxx.Begin < DateTime.Now && xxx.End > DateTime.Now && xxx.Status == true).ToList();
            ViewBag.ProducerList = cntx_.producernames.ToList();          

            return View(ListActionInProducers);

        }

    }
}