using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class PromotionController : BaseController
	{
		[HttpGet]
		public ActionResult Index()
		{
			var h = new NamesHelper(CurrentUser.Id);
			var producerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
			producerList.AddRange(h.RegisterListProducer());
			ViewBag.ProducerList = producerList;

			return View(new SearchProducerPromotion());
		}

		/// <summary>
		/// Поиск промоакций по фильтру
		/// </summary>
		public ActionResult SearchResult(SearchProducerPromotion filter)
		{
			var query = DB2.Promotions.AsQueryable();
			if (!filter.EnabledDateTime)
				query = query.Where(x => x.Begin > filter.Begin && x.End < filter.End);
			if (filter.Producer > 0)
				query = query.Where(x => x.ProducerId == filter.Producer);

			var model = query.OrderByDescending(x => x.UpdateTime).ToList();
			if (filter.Status != null)
				model = model.Where(x => x.GetStatus() == filter.Status.Value).ToList();

			var h = new NamesHelper(CurrentUser.Id);
			var producerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
			producerList.AddRange(h.RegisterListProducer());
			ViewBag.ProducerList = producerList;
			return PartialView("Partial/SearchResult", model);
		}

		/// <summary>
		/// Подробнее
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		public ActionResult View(long id)
		{
			var model = DB2.Promotions.Find(id);
			var h = new NamesHelper(CurrentUser.Id);
			ViewBag.ProducerName = h.GetProducerList().Single(x => x.Value == model.ProducerId.ToString()).Text;
			ViewBag.RegionList = h.GetPromotionRegionNames((ulong)model.RegionMask);
			ViewBag.DrugList = h.GetDrugInPromotion(model.Id);
			ViewBag.SupplierList = h.GetSupplierList(model.PromotionsToSupplier.ToList().Select(x => (decimal)x.SupplierId).ToList());
			ViewBag.History = DB2.PromotionHistory.Where(x => x.Promotion.Id == model.Id).ToList();

			return View(model);
		}

		/// <summary>
		/// Подтверждение промоакции
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		public ActionResult Confirm(long id)
		{
			var model = DB2.Promotions.Find(id);
			model.Status = PromotionStatus.Confirmed;
			DB2.PromotionHistory.Add(new PromotionSnapshot(CurrentUser, model, DB, DB2) {
				SnapshotName = "Подтверждение промоакции"
			});
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.StatusPromotion, model);

			SuccessMessage("Промоакция подтверждена");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Отмена подтверждения промоакции
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		public ActionResult Reject(int id, string comment)
		{
			var model = DB2.Promotions.Find(id);
			model.Status = PromotionStatus.Rejected;
			DB2.PromotionHistory.Add(new PromotionSnapshot(CurrentUser, model, DB, DB2) {
				SnapshotName = "Отклонение промоакции",
				SnapshotComment = comment
			});
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.StatusPromotion, model, comment);

			SuccessMessage("Промоакция отклонена");
			return RedirectToAction("Index");
		}

		public ActionResult History(int id)
		{
			var model = DB2.PromotionHistory.Find(id);
			var old = DB2.PromotionHistory
				.Where(x => x.Id < model.Id && x.Promotion.Id == model.Promotion.Id)
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();
			model.CalculateChanges(old);
			ViewBag.Old = old;
			return View(model);
		}
	}
}