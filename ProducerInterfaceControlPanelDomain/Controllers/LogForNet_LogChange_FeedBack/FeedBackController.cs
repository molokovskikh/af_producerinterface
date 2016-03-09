using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;
using System.Collections.Generic;
using System;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class FeedBackController : MasterBaseController
    {
        public ActionResult Index()
        {       
            return View("Index");
        }

        public JsonResult GetFilter()
        {
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);

            FeedBackFilter FBF = FeedBackFunc.GetFilter();

            return Json(FBF, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FeedBackSearch(FeedBackFilter feedBackFilter)
        {
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);          
            var FeedBackListRet = FeedBackFunc.GetFeedBackList(feedBackFilter);

            if (FeedBackListRet.FeedList.Count() == 0)
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }

            return Json(FeedBackListRet, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FeedBackGetItem(int Id)
        {
            FeedBackFunction FeedBackFunc = new FeedBackFunction(currentUser: CurrentUser);
            FeedBackItemSelect FeedBackModelView = FeedBackFunc.GetOneFeedBack(Id);

            return Json(FeedBackModelView, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddCommentToFeedBack(int IdFeedBack, string CommentAdd, int StatusFeedBack)
        {
            var NewComment = new AccountFeedBackComment();
            NewComment.


            return Json("0", JsonRequestBehavior.AllowGet);
        }

    }
}