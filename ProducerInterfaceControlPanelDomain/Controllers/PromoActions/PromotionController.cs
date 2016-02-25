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

        [HttpGet]
        public ActionResult Index()
        {
            var ModelSearch = new SearchProducerPromotion();
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            var ProducerList = new List<OptionElement>() { new OptionElement { Text = "", Value = "0" } };
            ProducerList.AddRange(h.RegisterListProducer());
            ViewBag.ProducerList = ProducerList;

            ModelSearch = new SearchProducerPromotion();
            ModelSearch.Status = (byte)PromotionStatus.All;
            ModelSearch.Begin = DateTime.Now.AddDays(-30);
            ModelSearch.End = DateTime.Now.AddDays(30);
            ModelSearch.EnabledDateTime = false;
                    
            return View(ModelSearch);          
        }
               
        public ActionResult SearchResult(SearchProducerPromotion Filter)
        {
            var PromotionList = new List<promotions>();

            if (!Filter.EnabledDateTime)
            {
                PromotionList = cntx_.promotions.Where(xxx => xxx.Begin < Filter.End && xxx.End > Filter.Begin).ToList();
            }
         
            if (Filter.Producer > 0)
            {
                if (PromotionList.Count() == 0)
                {
                    PromotionList = cntx_.promotions.Where(x => x.ProducerId == Filter.Producer).ToList();
                }
                else
                {
                    PromotionList = PromotionList.Where(x => x.ProducerId == Filter.Producer).ToList();
                }               
            }

            if (Filter.Status == (byte)PromotionStatus.All)
            { }
            else
            {
                if (Filter.Status == (byte)PromotionStatus.СonfirmedFalse)
                {
                    PromotionList = PromotionList.Where(x => x.Status == false).ToList();
                }

                if (Filter.Status == (byte)PromotionStatus.Confirmed)
                {
                    PromotionList = PromotionList.Where(x => x.Status == true).ToList();
                }

                if (Filter.Status == (byte)PromotionStatus.ConfirmedNotBegin)
                {
                    PromotionList = PromotionList.Where(x => x.Status == true && x.Begin > System.DateTime.Now).ToList();
                }

                if (Filter.Status == (byte)PromotionStatus.ConfirmedNotView)
                {
                    PromotionList = PromotionList.Where(x => x.Status == true && x.Enabled == false).ToList();
                }

                if (Filter.Status == (byte)PromotionStatus.ConfirmedEnd)
                {
                    PromotionList = PromotionList.Where(x => x.Status == true && x.End < System.DateTime.Now).ToList();
                }
            }

            var CustomHelper = new ProducerInterfaceCommon.CustomHelpers.Promotions();
            var ListId = CustomHelper.GetRegionPromotions(1020540654650, 10, 10);

            PromotionList = ListId;

            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

            foreach (var ItemPromo in PromotionList)
            {
                ItemPromo.GetRegionnamesList();
                ItemPromo.DrugList = h.GetDrugInPromotion(ItemPromo.Id);
            }
            
            ViewBag.ProducerList = h.RegisterListProducer();

            return PartialView("Partial/SearchResult",PromotionList);
        }

        public ActionResult OnePromotion(int Id=0)
        {
            if (Id == 0)
            {
                return RedirectToAction("Index");
            }

            var promotionModel = cntx_.promotions.Find(Id);

            if (promotionModel == null)
            {
                return RedirectToAction("Index");
            }

            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_,CurrentUser.Id);

            ViewBag.ProducerName = h.GetProducerList().Where(x => x.Value == promotionModel.ProducerId.ToString()).First().Text;
            ViewBag.RegionList = h.GetPromotionRegionNames((ulong)promotionModel.RegionMask);
            ViewBag.DrugList = h.GetDrugInPromotion(promotionModel.Id);

            return View(promotionModel);
        }



        public FileResult GetFile(int Id)
        {
            var ReturnFile = cntx_.promotionsimage.Find(Id);
            return File(ReturnFile.ImageFile, ReturnFile.ImageType, ReturnFile.ImageName);

        }
    }
}