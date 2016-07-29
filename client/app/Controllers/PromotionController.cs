using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.Data.Entity;
using System.IO;
using System.Web.Mvc.Html;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.ViewModel.Interface.Promotion;

namespace ProducerInterface.Controllers
{
	public class PromotionController : BaseController
	{
		private NamesHelper h;
		protected long producerId;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			if (CurrentUser == null)
				return;

			h = new NamesHelper(CurrentUser.Id);
			if (CurrentUser.AccountCompany.ProducerId.HasValue)
				producerId = CurrentUser.AccountCompany.ProducerId.Value;
			else
			{
				ErrorMessage("Доступ в раздел Акции закрыт, так как вы не представляете кого-либо из производителей");
				filterContext.Result = Redirect("~");
			}
		}

		public ActionResult Index()
		{
			var filter = new PromotionFilter();
			if (Request.HttpMethod == "POST")
				UpdateModel(filter);
			filter.Find(DB, DB2, producerId);
			return View(filter);
		}

		public ActionResult Edit(int id)
		{
			ViewBag.Title = "Редактирование акции";
			ViewBag.LoadUrl = Url.Action("Load", new { id });
			return View(DB2.Promotions.Find(id));
		}

		public ActionResult Copy(int id)
		{
			ViewBag.Title = "Добавление акции";
			ViewBag.LoadUrl = Url.Action("LoadCopy", new { id });
			return View("Edit");
		}

		public ActionResult New()
		{
			ViewBag.Title = "Добавление акции";
			ViewBag.LoadUrl = Url.Action("LoadNew");
			return View("Edit");
		}

		[HttpPost]
		public JsonResult LoadCopy(int id)
		{
			return ToJson(new Promotion(DB2.Promotions.Find(id), CurrentUser));
		}

		public JsonResult LoadNew()
		{
			return ToJson(new Promotion(CurrentUser));
		}

		public JsonResult Load(int id)
		{
			return ToJson(DB2.Promotions.Find(id));
		}

		private JsonResult ToJson(Promotion model)
		{
			var viewModel = new PromotionUi();
			viewModel.DrugCatalogList = h.GetCatalogList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
			viewModel.RegionGlobalList = DB.Regions().Select(x => new TextValue(x)).ToList();
			viewModel.Id = model.Id;
			viewModel.Name = model.Name;
			viewModel.Annotation = model.Annotation;
			if (model.MediaFile != null) {
				viewModel.PromotionFileId = model.MediaFile.Id;
				viewModel.PromotionFileName = model.MediaFile.ImageName;
				viewModel.PromotionFileUrl = Url.Action("GetFile", "MediaFiles", new {model.MediaFile.Id});
			}

			if (model.Id == 0) {
				viewModel.Title = "Новая промоакция";
			} else {
				viewModel.Title = "Редактирование промоакции: " + model.Name;
			}

			viewModel.PromotionFileId = model.MediaFile?.Id;
			if (model.Begin < DateTime.Now)
				viewModel.Begin = DateTime.Now.ToString("dd.MM.yyyy");
			else
				viewModel.Begin = model.Begin.ToString("dd.MM.yyyy");

			if (model.End < DateTime.Now)
				viewModel.End = DateTime.Now.ToString("dd.MM.yyyy");
			else
				viewModel.End = model.End.ToString("dd.MM.yyyy");

			viewModel.DrugList = model.PromotionToDrug.ToList().Select(x => x.DrugId.ToString()).ToList();
			var regions = DB.Regions((ulong) model.RegionMask);
			viewModel.RegionList = regions.Select(x => x.Id.ToString()).ToList();

			if (model.AllSuppliers)
				viewModel.SuppierRegions = new List<string>() {"0"};
			else
				viewModel.SuppierRegions = model.PromotionsToSupplier.Select(x => x.SupplierId.ToString()).ToList();
			viewModel.SuppierRegionsList = h.GetSupplierList(regions.Select(x => x.Id).ToList())
				.Select(x => new TextValue {Text = x.Text, Value = x.Value})
				.ToList();
			viewModel.SuppierRegionsList.Add(new TextValue() {Text = "Все поставщики из выбранных регионов", Value = "0"});

			return Json(viewModel, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult GetUpdateSupplierList(List<decimal> Id)
		{
			if (Id == null || Id.Count() == 0)
				return Json(new List<TextValue>(), JsonRequestBehavior.AllowGet);
			var supplierListOptionElements = h.GetSupplierList(Id).ToList();
			var supplierList = supplierListOptionElements
					.Select(x => new TextValue { Text = x.Text, Value = x.Value })
					.ToList();
			supplierList.Add(new TextValue() { Text = "Все поставщики из выбранных регионов", Value = "0" });
			return Json(supplierList, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult EditSave(PromotionUi model)
		{
			if (model.Id == 0)
			{
				SaveNewPromotion(model);
				SuccessMessage("Промо акция добавлена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index");
			}
			else
			{
				ChangePromotion(model);
				SuccessMessage("Промо акция изменена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index");
			}
		}

		private long ChangePromotion(PromotionUi model)
		{
			var promotion = DB2.Promotions.Find(model.Id);
			var promotionToDrug = promotion.PromotionToDrug.ToList();
			foreach (var item in promotionToDrug)
			{
				var drugExsist = model.DrugList.Any(x => Convert.ToInt64(x) == item.DrugId);
				if (!drugExsist)
					DB2.PromotionToDrugs.Remove(item);
			}
			foreach (var item in model.DrugList)
			{
				var drugExsist = promotion.PromotionToDrug.Any(x => x.DrugId == Convert.ToInt64(item));
				if (!drugExsist)
				{
					PromotionToDrug newAddDrug = new PromotionToDrug()
					{
						DrugId = Convert.ToInt64(item),
						PromotionId = promotion.Id
					};
					DB2.PromotionToDrugs.Add(newAddDrug);
				}
			}

			var promoPromotionsToSupplier = promotion.PromotionsToSupplier.ToList();
			foreach (var item in promoPromotionsToSupplier)
			{
				var supllierExsist = model.SuppierRegions.Any(x => (ulong)Convert.ToInt64(x) == (ulong)item.SupplierId);
				if (!supllierExsist)
					DB2.PromotionsToSuppliers.Remove(item);
			}
			if (model.SuppierRegions.Contains("0"))
			{
				model.SuppierRegions = h.GetSupplierList(model.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				promotion.AllSuppliers = true;
			}
			else
				promotion.AllSuppliers = false;

			foreach (var item in model.SuppierRegions)
			{
				var supllierExsist = promotion.PromotionsToSupplier.Any(x => (ulong)x.SupplierId == (ulong)Convert.ToInt64(item));
				if (!supllierExsist)
				{
					var addNew = new PromotionsToSupplier() {
						SupplierId = Convert.ToInt64(item),
						PromotionId = promotion.Id
					};
					promotion.PromotionsToSupplier.Add(addNew);
				}
			}

			ulong regionMask = 0;
			if (model.RegionList.Count() == 1)
				regionMask = (ulong)Convert.ToInt64(model.RegionList.First());
			else
				regionMask = model.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);

			promotion.RegionMask = (long)regionMask;
			promotion.Name = model.Name;
			promotion.Annotation = model.Annotation;
			promotion.Begin = Convert.ToDateTime(model.Begin);
			promotion.End = Convert.ToDateTime(model.End);
			promotion.Enabled = true;
			promotion.Status = PromotionStatus.New;
			promotion.Author = DB2.Users.Find(CurrentUser.Id);

			var file = SaveFile(model.File);
			if (file != null)
			{
				promotion.MediaFile = file;
			}
			else if (model.PromotionFileId != null) {
				promotion.MediaFile = DB2.MediaFiles.Find(model.PromotionFileId.Value);
			}
			DB2.PromotionHistory.Add(new PromotionSnapshot(CurrentUser, promotion, DB, DB2));
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.EditPromotion, promotion);
			return promotion.Id;
		}

		private long SaveNewPromotion(PromotionUi model)
		{
			var file = SaveFile(model.File);
			var regionMask = model.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);
			if (file == null)
			{
				if (model.PromotionFileId != null)
					file = DB2.MediaFiles.Find(model.PromotionFileId.Value);
			}

			var promotion = new Promotion(CurrentUser) {
				Name = model.Name,
				Annotation = model.Annotation,
				Begin = Convert.ToDateTime(model.Begin),
				End = Convert.ToDateTime(model.End),
				Author = DB2.Users.Find(CurrentUser.Id),
				MediaFile = file,
				RegionMask = (long)regionMask,
			};
			DB2.Promotions.Add(promotion);
			DB2.SaveChanges();

			foreach (var item in model.DrugList)
			{
				var promotionToDrug = new PromotionToDrug() {
					DrugId = Convert.ToInt64(item),
					PromotionId = promotion.Id
				};
				DB2.PromotionToDrugs.Add(promotionToDrug);
			}

			if (model.SuppierRegions.Contains("0")) {
				model.SuppierRegions = h.GetSupplierList(model.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				promotion.AllSuppliers = true;
			}
			else
				promotion.AllSuppliers = false;

			foreach (var item in model.SuppierRegions)
			{
				var X = new PromotionsToSupplier() { PromotionId = promotion.Id, SupplierId = Convert.ToInt64(item) };
				DB2.PromotionsToSuppliers.Add(X);
			}
			DB2.PromotionHistory.Add(new PromotionSnapshot(CurrentUser, promotion, DB, DB2) {
				SnapshotName = "Добавление промоакции",
			});
			DB2.SaveChanges();
			Mails.PromotionNotification(MailType.CreatePromotion, promotion);
			return promotion.Id;
		}

		private MediaFile SaveFile(HttpPostedFileBase src)
		{
			if (src == null)
				return null;

			var file = new MediaFile(src.FileName);
			var ms = new MemoryStream();
			src.InputStream.CopyTo(ms);

			file.ImageFile = ms.ToArray();
			file.ImageType = src.ContentType;
			file.ImageSize = ms.Length;
			file.EntityType = EntityType.Promotion;
			DB2.MediaFiles.Add(file);
			DB2.SaveChanges();
			return file;
		}

		/// <summary>
		/// Запуск-остановка публикации акции
		/// </summary>
		/// <param name="Id">идентификатор акции</param>
		/// <param name="Enabled">статус</param>
		[HttpGet]
		public ActionResult Publication(long Id, bool Enabled)
		{
			var promotion = DB2.Promotions.SingleOrDefault(x => x.Id == Id);
			if (promotion == null)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}
			;
			if (!promotion.CheckSecurity(CurrentUser))
			{
				ErrorMessage("У вас нет прав для изменения данной акции");
				return RedirectToAction("Index");
			}

			promotion.Enabled = Enabled;

			DB2.PromotionHistory.Add(new PromotionSnapshot(CurrentUser, promotion, DB, DB2) {
				SnapshotName = "Изменения статуса акции"
			});
			DB2.SaveChanges();

			Mails.PromotionNotification(MailType.EditPromotion, promotion);
			var message = promotion.GetStatus().DisplayName();
			SuccessMessage($"Новый статус: {message}");
			return RedirectToAction("Index", new { Id = Id.ToString() });
		}

		public ActionResult Delete(long id)
		{
			var promotion = DB2.Promotions.Find(id);
			if (promotion == null)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}
			if (!promotion.CheckSecurity(CurrentUser)) {
				ErrorMessage("У вас нет прав для удаления данной акции");
				return RedirectToAction("Index");
			}

			if (promotion.MediaFile != null)
			{
				DB2.MediaFiles.Remove(promotion.MediaFile);
				promotion.MediaFile = null;
			}

			var suppliers = DB2.PromotionsToSuppliers.Where(x => x.PromotionId == promotion.Id).ToList();
			foreach (var item in suppliers)
				DB2.PromotionsToSuppliers.Remove(item);
			var drugList = DB2.PromotionToDrugs.Where(x => x.PromotionId == promotion.Id).ToList();
			foreach (var item in drugList)
				DB2.PromotionToDrugs.Remove(item);

			DB2.Promotions.Remove(promotion);
			DB2.SaveChanges();
			SuccessMessage("Акция удалена");
			return RedirectToAction("Index");
		}

		public JsonResult GetSupplierJson(List<decimal> RegionList, List<long> SuppierRegions)
		{
			if (SuppierRegions == null)
				SuppierRegions = new List<long>();

			var supplierStringList = SuppierRegions.Select(x => x.ToString()).ToList();
			return Json(new
			{
				results = (h.GetSupplierList(RegionList).Select(x => new { value = x.Value, text = x.Text, selected = supplierStringList.Contains(x.Value) }))
			}, JsonRequestBehavior.AllowGet);
		}
	}
}