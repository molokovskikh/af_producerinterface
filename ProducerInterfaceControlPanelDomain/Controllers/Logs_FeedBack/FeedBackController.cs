using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;
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

		// возвращает модель фильтра
		public JsonResult GetFilter()
		{
			var feedBackFunc = new FeedBackFunction();
			var filter = feedBackFunc.GetFilter();
			return Json(filter, JsonRequestBehavior.AllowGet);
		}

		// возвращает коллекцию элементов по фильтру
		public JsonResult FeedBackSearch(FeedBackFilter feedBackFilter)
		{
			var feedBackFunc = new FeedBackFunction();
			var feedBackListRet = feedBackFunc.GetFeedBackList(feedBackFilter);

			if (feedBackListRet.FeedList == null || !feedBackListRet.FeedList.Any())
				return Json("0", JsonRequestBehavior.AllowGet);

			return Json(feedBackListRet, JsonRequestBehavior.AllowGet);
		}

		public JsonResult FeedBackGetItem(int Id)
		{
			var feedBackFunc = new FeedBackFunction();
			var feedBackModelView = feedBackFunc.GetOneFeedBack(Id);

			return Json(feedBackModelView, JsonRequestBehavior.AllowGet);
		}

		// изменение статуса обращения
		public JsonResult AddCommentToFeedBack(long Id, string Comment, FeedBackStatus Status)
		{
			var feedBackItem = cntx_.AccountFeedBack.Find(Id);
			feedBackItem.Comment = Comment;
			feedBackItem.DateEdit = DateTime.Now;
			feedBackItem.AdminId = CurrentUser.Id;
			feedBackItem.StatusEnum = Status;
			cntx_.SaveChanges(CurrentUser, "Добавление комментария к обращению пользователя");

			return Json("1", JsonRequestBehavior.AllowGet);
		}

	}
}