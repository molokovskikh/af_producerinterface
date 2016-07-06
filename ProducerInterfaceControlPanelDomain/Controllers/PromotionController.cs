using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class PromotionController : MasterBaseController
	{
		/// <summary>
		/// Список промоакций
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Index()
		{
			var h = new NamesHelper(DB, CurrentUser.Id);
			var producerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
			producerList.AddRange(h.RegisterListProducer());
			ViewBag.ProducerList = producerList;

			var model = new SearchProducerPromotion() {
				Status = (byte)PromotionFakeStatus.All,
				Begin = DateTime.Now.AddDays(-30),
				End = DateTime.Now.AddDays(30),
				EnabledDateTime = false
			};

			return View(model);
		}

		/// <summary>
		/// Поиск промоакций по фильтру
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public ActionResult SearchResult(SearchProducerPromotion filter)
		{
			var query = DB.promotions.AsQueryable();
			if (!filter.EnabledDateTime)
				query = query.Where(x => x.Begin > filter.Begin && x.End < filter.End);
			if (filter.Producer > 0)
				query = query.Where(x => x.ProducerId == filter.Producer);

			switch ((PromotionFakeStatus)filter.Status) {
				// отключена пользователем
				case PromotionFakeStatus.Disabled:
					query = query.Where(x => !x.Enabled);
					break;
				// отклонена админом
				case PromotionFakeStatus.Rejected:
					query = query.Where(x => x.Status == (byte)PromotionStatus.Rejected);
					break;
				// ожидает подтверждения
				case PromotionFakeStatus.NotConfirmed:
					query = query.Where(x => x.Enabled && x.Status == (byte)PromotionStatus.New);
					break;
				// не началась
				case PromotionFakeStatus.ConfirmedNotBegin:
					query = query.Where(x => x.Enabled && x.Status == (byte)PromotionStatus.Confirmed && x.Begin > DateTime.Now);
					break;
				// закончилась
				case PromotionFakeStatus.ConfirmedEnded:
					query = query.Where(x => x.Enabled && x.Status == (byte)PromotionStatus.Confirmed && x.End < DateTime.Now);
					break;
				// активна
				case PromotionFakeStatus.Active:
					query = query.Where(x => x.Enabled && x.Status == (byte)PromotionStatus.Confirmed && x.Begin < DateTime.Now && x.End > DateTime.Now);
					break;
				case PromotionFakeStatus.All:
					break;
			}

			var model = query.OrderByDescending(x => x.UpdateTime).ToList();

			var h = new NamesHelper(DB, CurrentUser.Id);
			var producerList = new List<OptionElement>() { new OptionElement { Text = "Все зарегистрированные", Value = "0" } };
			producerList.AddRange(h.RegisterListProducer());
			ViewBag.ProducerList = producerList;
			return PartialView("Partial/SearchResult", model);
		}

		/// <summary>
		/// Подробнее
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		/// <returns></returns>
		public ActionResult OnePromotion(int Id = 0)
		{
			if (Id == 0)
				return RedirectToAction("Index");
			var model = DB.promotions.Find(Id);
			if (model == null)
				return RedirectToAction("Index");

			var h = new NamesHelper(DB, CurrentUser.Id);

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
		/// <returns></returns>
		public ActionResult SuccessPromotion(int Id = 0)
		{
			if (Id == 0)
				return RedirectToAction("Index");
			var model = DB.promotions.Single(x => x.Id == Id);
			if (model == null)
				return RedirectToAction("Index");

			model.Status = (byte)PromotionStatus.Confirmed;
			DB.SaveChanges(CurrentUser, "Подтверждение промоакции");

			SuccessMessage("Промоакция подтверждена");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Отмена подтверждения промоакции
		/// </summary>
		/// <param name="Id">идентификатор промоакции</param>
		/// <returns></returns>
		public ActionResult UnSuccessPromotion(int Id = 0)
		{
			if (Id == 0)
				return RedirectToAction("Index");
			var model = DB.promotions.Single(x => x.Id == Id);
			if (model == null)
				return RedirectToAction("Index");

			model.Status = (byte)PromotionStatus.New;
			DB.SaveChanges(CurrentUser, "Подтверждение промоакции");

			SuccessMessage("Промоакция отменено подтверждение");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public FileResult GetFile(int Id)
		{
			var file = DB.MediaFiles.Find(Id);
			return File(file.ImageFile, file.ImageType, file.ImageName);
		}
	}
}