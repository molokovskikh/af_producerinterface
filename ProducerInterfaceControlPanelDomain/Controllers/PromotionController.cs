using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PromotionController : BaseController
    {
        // GET: Promotion
        public ActionResult Index(bool EnabledPromotion = false, long ProducerId = 0)
        {
            var ListPromotion = new  List<Models.promotions>();          
            var UserId = cntx_.ControlPanelUser.Where(xxx => xxx.Name == currentUser).First();
         
            if (ProducerId == 0)
            {
                // Глобальный список акций (по умолчанию, не подтверждённых)
                ViewBag.EnabledPromotion = EnabledPromotion;
                ListPromotion = cntx_.promotions.Where(xxx => xxx.Enabled == EnabledPromotion).ToList();                                 
            }
            else
            {
                // Акции производителя
               // ViewBag.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == ProducerId).First().ProducerName;
                ListPromotion = cntx_.promotions.Where(xxx => xxx.Enabled == EnabledPromotion).Where(xxx=>xxx.ProducerId==ProducerId).ToList();
            }

            ViewBag.ListDrugs = cntx_.catalognames.ToList();
            ViewBag.ProducerList = cntx_.producernames.ToList();
            return View(ListPromotion);
        }

        [HttpGet]
        public ActionResult Search()
        {
            var ViewModel =new Models.SearchPromotion();
            var ProducerList = new List<Models.OptionElement>() { new Models.OptionElement { Text = "", Value = "" } };
            ProducerList.AddRange(cntx_.producernames.ToList().Select(xxx =>
                                   new Models.OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList());
            ViewBag.ProducerList = ProducerList;            
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult Search(Models.SearchPromotion IdProducers)
        {
            if (!ModelState.IsValid)
            {
                var ProducerList = new List<Models.OptionElement>() { new Models.OptionElement { Text = "", Value = "" }};             
                ProducerList.AddRange(cntx_.producernames.ToList().Select(xxx =>
                                       new Models.OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList());
                ViewBag.ProducerList = ProducerList;

                var ViewModel = new Models.SearchPromotion();
                
                return View("Search", ViewModel);
            }

            return RedirectToAction("Index", new { ProducerId = IdProducers.IdProducer });
        }

        [HttpGet]
        public ActionResult Edit(long Id)
        {
            var PromoActionModel = cntx_.promotions.Where(xxx => xxx.Id == Id).First();
            
            ViewBag.ListDrugs = cntx_.catalognames.ToList();
            ViewBag.ProducerList = cntx_.producernames.Where(xxx => xxx.ProducerId == PromoActionModel.ProducerId).First();
            return View(PromoActionModel);
        }
        public ActionResult Edit(Models.promotions PromotionSuccess)
        {
            var promotionUpdate = cntx_.promotions.Where(xxx => xxx.Id == PromotionSuccess.Id).First();
            var ContolUserName = GetUserName();
            var ControlUser = cntx_.ControlPanelUser.Where(xxx => xxx.Name == ContolUserName).First().Id;

            promotionUpdate.Enabled = true;
            promotionUpdate.AdminId = ControlUser;
            promotionUpdate.Status = true;

            cntx_.Entry(promotionUpdate).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Акция " + promotionUpdate.Name + " подтверждена");        
            return RedirectToAction("Index", new { EnabledPromotion=true });
        }

        public ActionResult Active()
        {
            var ListPromotion = cntx_.promotions.Where(xxx => xxx.End > DateTime.Now && xxx.Begin < DateTime.Now && xxx.Status == true && xxx.Enabled ==true).ToList();
            ViewBag.ListDrugs = cntx_.catalognames.ToList();
            ViewBag.ProducerList = cntx_.producernames.ToList();
            return View("Index",ListPromotion);
        }

    }
}