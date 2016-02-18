using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.Global
{
    public class FeedBackController : MasterBaseController
    {
        // GET: FeedBack
        public ActionResult Index()
        {
            var FeedBackList = cntx_.AccountFeedBack.Where(xxx => xxx.Status == 0).ToList();

            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();


            return View(FeedBackList);
        }

      


    }
}