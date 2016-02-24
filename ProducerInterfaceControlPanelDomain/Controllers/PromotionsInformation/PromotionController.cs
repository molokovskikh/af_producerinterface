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

            ModelSearch.Status = 5;
            ModelSearch.Begin = DateTime.Now.AddDays(-30);
            ModelSearch.End = DateTime.Now.AddDays(30);
            ModelSearch.PagerInt = 0;

            return View(ModelSearch);
          
        }
            
        public ActionResult SearchResult(SearchProducerPromotion Filter)
        {
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();

            var PromotionList = cntx_.promotions.ToList();

            foreach (var ItemPromo in PromotionList)
            {
                ItemPromo.GetRegionnamesList();
                ItemPromo.DrugList = h.GetDrugInPromotion(ItemPromo.Id);
            }

            ViewBag.PromotionDrugList = h.GetCatalogListPromotion();


            return PartialView(PromotionList);
        }

    }
}