using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PromotionController : MasterBaseController
    {
        // GET: Promotion
        public ActionResult Index(bool EnabledPromotion = false, long ProducerId = 0)
        {
            var ListPromotion = new  List<promotions>();          
            var UserId = cntx_.Account.Where(xxx => xxx.Login == CurrentUser.Login).First();
         
            if (ProducerId == 0)
            {
                // Глобальный список акций (по умолчанию, не подтверждённых)
                ViewBag.EnabledPromotion = EnabledPromotion;
                ListPromotion = cntx_.promotions.Where(xxx => xxx.Status == EnabledPromotion).ToList();                                 
            }
            else
            {
                // Акции производителя
                ViewBag.ProducerName = cntx_.producernames.Where(xxx => xxx.ProducerId == ProducerId).First().ProducerName;
                ListPromotion = cntx_.promotions.Where(xxx => xxx.Status == EnabledPromotion).Where(xxx=>xxx.ProducerId==ProducerId).ToList();
            }

            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            foreach (var PromoItem in ListPromotion)
            {
              
                PromoItem.RegionList = h.GetPromotionRegions((ulong)PromoItem.RegionMask);
            }

            ViewBag.RegionList = h.GetRegionList();
            ViewBag.ListDrugs = cntx_.catalognames.ToList();
            ViewBag.ProducerList = cntx_.producernames.ToList();

            return View(ListPromotion);
        }

        [HttpGet]
        public ActionResult Search()
        {
            var ViewModel =new SearchPromotion();
            var ProducerList = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
            ProducerList.AddRange(cntx_.producernames.ToList().Select(xxx =>
                                   new OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList());
            ViewBag.ProducerList = ProducerList;            
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult Search(SearchPromotion IdProducers)
        {
            if (!ModelState.IsValid)
            {
                var ProducerList = new List<OptionElement>() { new OptionElement { Text = "", Value = "" }};             
                ProducerList.AddRange(cntx_.producernames.ToList().Select(xxx =>
                                       new OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList());
                ViewBag.ProducerList = ProducerList;

                var ViewModel = new SearchPromotion();
                
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

        public ActionResult Edit(promotions PromotionSuccess)
        {
            var promotionUpdate = cntx_.promotions.Where(xxx => xxx.Id == PromotionSuccess.Id).First();
            var ContolUserName = CurrentUser.Login;
            var ControlUser = cntx_.Account.Where(xxx => xxx.Login == ContolUserName && xxx.TypeUser == SbyteTypeUser).First().Id;

            promotionUpdate.Status = true;
        //    promotionUpdate.AdminId = ControlUser;      
                   
            cntx_.Entry(promotionUpdate).State = System.Data.Entity.EntityState.Modified;
            cntx_.SaveChanges();

            SuccessMessage("Акция " + promotionUpdate.Name + " подтверждена");        
            return RedirectToAction("Index", new { EnabledPromotion=true });
        }

        public ActionResult Active()
        {
            var ListPromotion = cntx_.promotions.Where(xxx => xxx.End > DateTime.Now && xxx.Begin < DateTime.Now && xxx.Status ==true && xxx.Enabled == true).ToList();
            ViewBag.ListDrugs = cntx_.catalognames.ToList();
            ViewBag.ProducerList = cntx_.producernames.ToList();
            ViewBag.ActivePromo = "Подтвержденные и Активные на данный момент Промо-Акции";
            return View("Index",ListPromotion);
        }

        public FileResult GetFile(int Id)
        {
            var File_ = cntx_.promotionsimage.Where(xxx => xxx.Id == Id).First();
            return File(File_.ImageFile, File_.ImageType, File_.ImageName);
        }

    }
}