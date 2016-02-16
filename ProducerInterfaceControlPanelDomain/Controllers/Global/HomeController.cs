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
            var CompanyList = cntx_.AccountCompany.ToList();
            var producerLongList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId != null).ToList().Select(xxx => xxx.ProducerId).ToList();
            ViewBag.ProducerList = cntx_.producernames.Where(xxx=> producerLongList.Contains(xxx.ProducerId)).ToList();
            ViewBag.ListPromotions = cntx_.promotions.ToList();

            return View(CompanyList);
        }

    }
}