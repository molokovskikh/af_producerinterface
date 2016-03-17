using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.CustomHelpers.Func;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PromotionController : MasterBaseController
    {

        [HttpGet]
        public ActionResult Index()
        {
            var ModelSearch = new SearchProducerPromotion();
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            var ProducerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
            ProducerList.AddRange(h.RegisterListProducer());
            ViewBag.ProducerList = ProducerList;
            ViewBag.RegionList = h.GetRegionList((decimal)CurrentUser.RegionMask);
            
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

            SqlProcedure<ProducerInterfaceCommon.CustomHelpers.Models.PromotionsInRegionMask> CH = new SqlProcedure<ProducerInterfaceCommon.CustomHelpers.Models.PromotionsInRegionMask>((ulong)CurrentUser.RegionMask);
            var ListId = CH.GetPromotionId();
                    
            PromotionList = cntx_.promotions.Where(x => ListId.Contains(x.Id)).ToList();
                       
            if (!Filter.EnabledDateTime)
            {
                PromotionList = PromotionList.Where(x=> x.Begin > Filter.Begin && x.End < Filter.End).ToList();
            }

            if (Filter.Producer > 0)
            {
                PromotionList = PromotionList.Where(x => x.ProducerId == Filter.Producer).ToList();
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

            PromotionList = PromotionList.OrderByDescending(x => x.UpdateTime).ToList();

            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

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

        public ActionResult SuccessPromotion(int Id = 0)
        {
            if (Id == 0)
            {
                return RedirectToAction("Index");
            }
            var promotionModel = cntx_.promotions.Where(xxx=>xxx.Id == Id).First();

            if (promotionModel == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                promotionModel.Status = true;
                cntx_.Entry(promotionModel).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges(CurrentUser, "Подтверждение промоакции");

                SuccessMessage("Промоакция подтверждена");
                return RedirectToAction("Index");
            }
        }

        public ActionResult UnSuccessPromotion(int Id = 0)
        {
            if (Id == 0)
            {
                return RedirectToAction("Index");
            }
            var promotionModel = cntx_.promotions.Where(xxx => xxx.Id == Id).First();

            if (promotionModel == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                promotionModel.Status = false;
                cntx_.Entry(promotionModel).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges(CurrentUser, "Подтверждение промоакции");

                SuccessMessage("Промоакция отменено подтверждение");
                return RedirectToAction("Index");
            }
        }

        public FileResult GetFile(int Id)
        {
            var ReturnFile = cntx_.MediaFiles.Find(Id);
            return File(ReturnFile.ImageFile, ReturnFile.ImageType, ReturnFile.ImageName);
        }
    }
}