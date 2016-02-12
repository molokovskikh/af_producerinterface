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
        private long? producer_ID;

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            if (producer_ID != null && producer_ID > 0)
            {
                ViewBag.ProducerNames = cntx_.producernames.Where(xxx => xxx.ProducerId == producer_ID).First().ProducerName;
                ViewBag.CompanyInformation = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == producer_ID).First();
                ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            }
        }

        //[HttpGet]
        //public ActionResult Index()
        //{



        //    return View("Search");
        //}

      //  [HttpOptions]               
        public ActionResult Index(long Id = 953)
        {
            producer_ID = Id;
            return View();



        }


        // Get Producer Infarmation

        [HttpGet]
        public ActionResult GetPromotion(long Id = 953)
        {
            producer_ID = Id;
            if (HttpContext.Request.IsAjaxRequest())
            {
                ViewBag.LayoutUsed = false;
            }
            else
            {
                ViewBag.LayoutUsed = true;
            }
            return View("PromotionPartial");
        }




    }
}