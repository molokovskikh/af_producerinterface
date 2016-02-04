using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class AccountController : MasterBaseController
    {
        // GET: Account
        public ActionResult Index()
        {
            RegistrationAccountValidation ModelView = new RegistrationAccountValidation();
            ViewBag.ProducerList = cntx_.producernames.Select(xxx => new OptionElement { Text = xxx.ProducerName, Value=xxx.ProducerId.ToString() }).ToList();
            return View(ModelView);
        }

        public ActionResult Registration(RegistrationAccountValidation One)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", One);
            }

            RegistrerValidation ModelView = new RegistrerValidation();

            ModelView.Producers = One.Producers;
            ModelView.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == One.Producers).First().ProducerName;
         

            return View(ModelView);
        }

        public ActionResult CustomRegistration()
        {
            return View();
        }

    }
}