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

        public JsonResult AddCommentToFeedBack(int IdFeedBack, string CommentAdd, int StatusFeedBack = 1000000)
        {

            var FeedBackItem = cntx_.AccountFeedBack.Find(IdFeedBack);
            
            var NewComment = new AccountFeedBackComment();
            NewComment.IdFeedBack = IdFeedBack;
            NewComment.Comment = CommentAdd;
            NewComment.DateAdd = DateTime.Now;
            NewComment.AdminId = CurrentUser.Id;

            if (StatusFeedBack != 1980)
            {
                NewComment.StatusNew = (sbyte)StatusFeedBack;
            }
            NewComment.StatusOld = FeedBackItem.Status;
            
            cntx_.Entry(NewComment).State = System.Data.Entity.EntityState.Added;
            cntx_.SaveChanges(CurrentUser,"Добавление комментария к обращению пользователя");

            if (StatusFeedBack != 1000000)
            {
                FeedBackItem.Status = (sbyte)StatusFeedBack;
                cntx_.Entry(FeedBackItem).State = System.Data.Entity.EntityState.Modified;
                cntx_.SaveChanges(CurrentUser, "Изменение статуса обращения пользователя");
            }

            return Json("1", JsonRequestBehavior.AllowGet);
        }

    }
}