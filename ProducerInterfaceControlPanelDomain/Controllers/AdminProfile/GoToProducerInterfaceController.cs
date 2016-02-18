﻿using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
    public class GoToProducerInterfaceController : MasterBaseController
    {
   
        private ProducerInterfaceCommon.Heap.NamesHelper h;

        // GET: GoToProducerInterface
        [HttpGet]
        public ActionResult Index()
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            var ProducerList = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
            var ProducerListRegistration = h.RegisterListProducer();
            ProducerList.AddRange(ProducerListRegistration);           
            return View(ProducerList);
        }

        [HttpPost]
        public ActionResult Index(long idproducer, long? produceruserid)
        {
            if (produceruserid == null)
            {
                ErrorMessage("Не выбран пользователь");
                return RedirectToAction("index");
            }

            Guid G1 = Guid.NewGuid();
            Guid G2 = Guid.NewGuid();

            var match = System.Text.RegularExpressions.Regex.Match(G1.ToString(), @"\-?\d+(\.\d{0,})?");
            var match2 = System.Text.RegularExpressions.Regex.Match(G2.ToString(), @"[0-9][0-9]+(?:\.[0-9]*)?");
            
            var i = match +  (CurrentUser.Name.Length * 19801112).ToString() + match2;
            var Url = GetWebConfigParameters("GoToProducerUserUrl");

            var UrlRedirect = Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" + produceruserid;

            return Redirect(UrlRedirect);            
        }

        public JsonResult GetListUser(long idproducer)
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            var UserList = h.GetProducerUserList(idproducer);
            return Json(UserList.Select(x=> new { value=x.Value, text= x.Text}), JsonRequestBehavior.AllowGet);
        }


  

    }
}