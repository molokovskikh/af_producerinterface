using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;
using System;
using System.Collections.Generic;

namespace ProducerInterfaceControlPanelDomain.Controllers.Global
{
    public class FeedBackController : MasterBaseController
    {
        // GET: FeedBack
        public ActionResult Index()
        {
            SearchModel FeedBackSearch = new SearchModel();

            FeedBackSearch.Begin = DateTime.Now.AddMonths(-1);
            FeedBackSearch.End = DateTime.Now.AddDays(1);

            FeedBackSearch.DateTimeApply = true;
            FeedBackSearch.Producer = false;
            FeedBackSearch.ProducerId = 0;

            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();

            return View(FeedBackSearch);
        }
        
        public ActionResult FeedBackSearch(SearchModel FeedBackSearchModel)
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            ViewBag.ProducerList = h.RegisterListProducer();

            List<ProducerInterfaceCommon.ContextModels.AccountFeedBack> ResultSearchModel = new List<ProducerInterfaceCommon.ContextModels.AccountFeedBack>();

            if (!FeedBackSearchModel.DateTimeApply && !FeedBackSearchModel.Producer)
            {
                ResultSearchModel = cntx_.AccountFeedBack.OrderByDescending(x => x.DateAdd).Take(100).ToList();
                return PartialView(ResultSearchModel);
            }
        
            if (FeedBackSearchModel.DateTimeApply)
            {
                ResultSearchModel = cntx_.AccountFeedBack.Where(x => x.DateAdd >= FeedBackSearchModel.Begin && x.DateAdd <= FeedBackSearchModel.End).ToList();
            }
            if (FeedBackSearchModel.Producer)
            {
                if (ResultSearchModel.Count() == 0)
                {
                    ResultSearchModel = cntx_.AccountFeedBack.Where(x => x.Account.AccountCompany.ProducerId == FeedBackSearchModel.ProducerId).ToList();
                }
                else
                {
                    ResultSearchModel = ResultSearchModel.Where(x=>x.Account != null).Where(x => x.Account.AccountCompany.ProducerId == FeedBackSearchModel.ProducerId).ToList();
                }
            }
          
            return PartialView(ResultSearchModel.OrderByDescending(x=>x.DateAdd).Take(100).ToList());
        }     
    }
}