using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class PromotionController : BaseController
	{
		/// <summary>
		/// Список промоакций
		/// </summary>
		[HttpGet]
		public ActionResult Index()
		{
			var h = new NamesHelper(CurrentUser.Id);
			var producerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
			producerList.AddRange(h.RegisterListProducer());
			ViewBag.ProducerList = producerList;

			var model = new SearchProducerPromotion() {
				Status = (byte)ActualPromotionStatus.All,
				Begin = DateTime.Now.AddDays(-30),
				End = DateTime.Now.AddDays(30),
				EnabledDateTime = false
			};

			return View(model);
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

			switch ((ActualPromotionStatus)filter.Status) {
				// отключена пользователем
				case ActualPromotionStatus.Disabled:
					query = query.Where(x => !x.Enabled);
					break;
				// отклонена админом
				case ActualPromotionStatus.Rejected:
					query = query.Where(x => x.Status == PromotionStatus.Rejected);
					break;
				// ожидает подтверждения
				case ActualPromotionStatus.NotConfirmed:
					query = query.Where(x => x.Enabled && x.Status == PromotionStatus.New);
					break;
				// не началась
				case ActualPromotionStatus.ConfirmedNotBegin:
					query = query.Where(x => x.Enabled && x.Status == PromotionStatus.Confirmed && x.Begin > DateTime.Now);
					break;
				// закончилась
				case ActualPromotionStatus.ConfirmedEnded:
					query = query.Where(x => x.Enabled && x.Status == PromotionStatus.Confirmed && x.End < DateTime.Now);
					break;
				// активна
				case ActualPromotionStatus.Active:
					query = query.Where(x => x.Enabled && x.Status == PromotionStatus.Confirmed && x.Begin < DateTime.Now && x.End > DateTime.Now);
					break;
				case ActualPromotionStatus.All:
					break;
			}

			var model = query.OrderByDescending(x => x.UpdateTime).ToList();

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
		public ActionResult View(long Id = 0)
		{
			var model = DB2.Promotions.Find(Id);

			var h = new NamesHelper(CurrentUser.Id);
			ViewBag.ProducerName = h.GetProducerList().Single(x => x.Value == model.ProducerId.ToString()).Text;
			ViewBag.RegionList = h.GetPromotionRegionNames((ulong)model.RegionMask);
			ViewBag.DrugList = h.GetDrugInPromotion(model.Id);
			ViewBag.SupplierList = h.GetSupplierList(model.PromotionsToSupplier.ToList().Select(x => (decimal)x.SupplierId).ToList());

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
			DB.SaveChanges(CurrentUser, "Подтверждение промоакции");
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.StatusPromotion,  model);

			SuccessMessage("Промоакция подтверждена");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Отмена подтверждения промоакции
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		public ActionResult Reject(int id)
		{
			var model = DB2.Promotions.Find(id);
			model.Status = PromotionStatus.Rejected;
			DB.SaveChanges(CurrentUser, "Отклонение промоакции");
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.StatusPromotion,  model);

			SuccessMessage("Промоакция отклонена");
			return RedirectToAction("Index");
		}

		public FileResult GetFile(int id)
		{
			var file = DB.MediaFiles.Find(id);
			return File(file.ImageFile, file.ImageType, file.ImageName);
		}
	}
}