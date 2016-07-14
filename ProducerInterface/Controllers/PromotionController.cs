using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.Data.Entity;
using System.IO;
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
				filterContext.Result = Redirect("~");
			else
			{
				h = new NamesHelper(CurrentUser.Id, HttpContext);
				if (CurrentUser.AccountCompany.ProducerId.HasValue)
					producerId = CurrentUser.AccountCompany.ProducerId.Value;
				else
				{
					ErrorMessage("Доступ в раздел Акции закрыт, так как вы не представляете кого-либо из производителей");
					filterContext.Result = Redirect("~");
				}
			}
		}

		/// <summary>
		/// Акции главная
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public ActionResult Index(string Id = null)
		{
			var model = new List<PromotionUi>();
			var promoList = DB2.Promotions.Where(x => x.ProducerId == producerId).ToList();
			if (promoList.Count == 0)
				return View(model);

			var suppliers = DB.suppliernames.ToDictionary(x => x.SupplierId, x => x.SupplierName);
			var assortment = DB.assortment.Where(x => x.ProducerId == producerId).ToDictionary(x => x.CatalogId, x => x.CatalogName);
			foreach (var item in promoList) {
				if (item.RegionMask == 0)
					item.RegionMask = ulong.MaxValue;
				var supplierIds = item.PromotionsToSupplier.Select(x => x.SupplierId).ToList();
				var drugsIds = item.promotionToDrug.Select(x => x.DrugId).ToList();
				var itemUi = new PromotionUi() {
					Id = item.Id,
					Name = item.Name,
					Annotation = item.Annotation,
					Begin = item.Begin.ToString("dd.MM.yyyy"),
					End = item.End.ToString("dd.MM.yyyy"),
					PromotionFileId = item.MediaFile?.Id,
					PromotionFileName = item.MediaFile?.ImageName,
					AllSuppliers = item.AllSuppliers,
					FakeStatus = item.GetStatus(),
					DrugList = assortment.Where(x => drugsIds.Contains(x.Key)).Select(x => x.Value).ToList(),
					RegionList = DB.Regions((ulong)item.RegionMask).Select(x => x.Name).ToList(),
					SuppierRegions = suppliers.Where(x => supplierIds.Contains(x.Key)).Select(x => x.Value).ToList()
				};
				model.Add(itemUi);
			}

			if (Id != null)
				ViewBag.OpenPromotionId = Id;
			return View(model);
		}

		[HttpGet]
		public ActionResult Edit(long IdKey = 0, bool Copy = false)
		{
			var id = IdKey.ToString();
			if (Copy)
				id += ", true";
			ViewBag.PromotionId = id;
			return View();
		}

		[HttpPost]
		public JsonResult EditGetPromotion(string IdKey = "0")
		{
			var model = new PromotionUi();
			var id = Convert.ToInt64(IdKey.Split(',').First());
			if (IdKey.Contains("true"))
				id = 0;

			model.DrugCatalogList = h.GetCatalogList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
			model.RegionGlobalList = h.GetRegionList().ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();

			if (IdKey == "0")
			{
				// возвращаем новую промо акцию
				model.Id = 0;
				model.Title = "Новая промоакция";
				model.Name = "";
				model.Annotation = "";
				model.Begin = DateTime.Now.ToString("dd.MM.yyyy");
				model.End = DateTime.Now.ToString("dd.MM.yyyy");
				model.SuppierRegionsList = h.GetSupplierList(new List<decimal>() { 0 }).Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
				model.RegionList = h.GetPromotionRegions(Convert.ToUInt64(0)).ToList().Select(x => x.ToString()).ToList();
				model.RegionGlobalList = h.GetRegionList().ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
			}
			else
			{
				var idPromo = Convert.ToInt64(IdKey.Split(new Char[] { ',' }).First());
				var dBmodel = DB2.Promotions.Find(idPromo);
				model.Id = id;
				model.SuppierRegions = dBmodel.PromotionsToSupplier.ToList().Select(x => (string)x.SupplierId.ToString()).ToList();
				model.SuppierRegionsList = h.GetSupplierList(model.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
				model.Name = dBmodel.Name;

				if (dBmodel.AllSuppliers)
					model.SuppierRegions = new List<string>() { "0" };

				if (id == 0)
				{
					model.Name += " Копия";
					model.Title = "Создание промоакции: копия из " + dBmodel.Name;
				}
				else
					model.Title = "Редактирование промоакции: " + dBmodel.Name;

				model.PromotionFileId = dBmodel.MediaFile?.Id;
				model.DrugList = dBmodel.promotionToDrug.ToList().Select(x => x.DrugId.ToString()).ToList();
				if (dBmodel.Begin < DateTime.Now)
					model.Begin = DateTime.Now.ToString("dd.MM.yyyy");
				else
					model.Begin = dBmodel.Begin.ToString("dd.MM.yyyy");

				if (dBmodel.End < DateTime.Now)
					model.End = DateTime.Now.ToString("dd.MM.yyyy");
				else
					model.End = dBmodel.End.ToString("dd.MM.yyyy");

				model.RegionList = h.GetPromotionRegions(Convert.ToUInt64(dBmodel.RegionMask)).ToList().Select(x => x.ToString()).ToList();
				model.SuppierRegionsList = h.GetSupplierList(model.RegionList.Select(x => (decimal)Convert.ToUInt64(x)).ToList()).ToList()
						.Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();

				//model.PromotionFileUrl = Url.Action("GetFile", new { id = dBmodel.MediaFiles?.Id });
				model.Annotation = dBmodel.Annotation;
			}

			model.SuppierRegionsList.Add(new TextValue() { Text = "Все поставщики из выбранных регионов", Value = "0" });

			return Json(model, JsonRequestBehavior.AllowGet);
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
				// добавить
				var idNewPromo = SaveNewPromotion(model);
				SuccessMessage("Промо акция добавлена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index", new { Id = idNewPromo });
			}
			else
			{
				var idNewPromo = ChangePromotion(model);
				SuccessMessage("Промо акция изменена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index", new { Id = idNewPromo });
			}
		}

		private long ChangePromotion(PromotionUi model)
		{
			var promoDb = DB2.Promotions.Find(model.Id);
			var promotionToDrug = promoDb.promotionToDrug.ToList();
			foreach (var item in promotionToDrug)
			{
				var drugExsist = model.DrugList.Any(x => Convert.ToInt64(x) == item.DrugId);
				if (!drugExsist)
					DB2.PromotionToDrugs.Remove(item);
			}
			DB.SaveChanges(CurrentUser, "Препарат удален из акции");
			foreach (var item in model.DrugList)
			{
				var drugExsist = promoDb.promotionToDrug.Any(x => x.DrugId == Convert.ToInt64(item));
				if (!drugExsist)
				{
					PromotionToDrug newAddDrug = new PromotionToDrug()
					{
						DrugId = Convert.ToInt64(item),
						PromotionId = promoDb.Id
					};
					DB2.PromotionToDrugs.Add(newAddDrug);
				}
			}
			DB.SaveChanges(CurrentUser, "Препарат добавлен в акцию");

			var promoPromotionsToSupplier = promoDb.PromotionsToSupplier.ToList();
			foreach (var item in promoPromotionsToSupplier)
			{
				var supllierExsist = model.SuppierRegions.Any(x => (ulong)Convert.ToInt64(x) == (ulong)item.SupplierId);
				if (!supllierExsist)
					DB2.PromotionsToSuppliers.Remove(item);
			}
			DB.SaveChanges(CurrentUser, "Поставщик удален из акции");
			if (model.SuppierRegions.Contains("0"))
			{
				model.SuppierRegions = h.GetSupplierList(model.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				promoDb.AllSuppliers = true;
			}
			else
				promoDb.AllSuppliers = false;

			foreach (var item in model.SuppierRegions)
			{
				var supllierExsist = promoDb.PromotionsToSupplier.Any(x => (ulong)x.SupplierId == (ulong)Convert.ToInt64(item));
				if (!supllierExsist)
				{
					var addNew = new PromotionsToSupplier() {
						SupplierId = Convert.ToInt64(item),
						PromotionId = promoDb.Id
					};
					promoDb.PromotionsToSupplier.Add(addNew);
				}
			}
			DB.SaveChanges(CurrentUser, "Поставщик добавлен в акцию");

			ulong regionMask = 0;
			if (model.RegionList.Count() == 1)
				regionMask = (ulong)Convert.ToInt64(model.RegionList.First());
			else
				regionMask = model.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);

			promoDb.RegionMask = regionMask;
			promoDb.Name = model.Name;
			promoDb.Annotation = model.Annotation;
			promoDb.Begin = Convert.ToDateTime(model.Begin);
			promoDb.End = Convert.ToDateTime(model.End);
			promoDb.AgencyDisabled = false;
			promoDb.Enabled = true;
			promoDb.Status = (byte)PromotionStatus.New;
			promoDb.Author = DB2.Users.Find(CurrentUser.Id);
			DB.Entry(promoDb).State = EntityState.Modified;
			DB.SaveChanges();

			var file = SaveFile(model.File);
			if (file != null)
			{
				var promoFileRemove = DB.MediaFiles.Find(promoDb.MediaFile.Id);
				promoDb.MediaFile = null;
				promoDb.PromoFile = "";
				DB.Entry(promoDb).State = EntityState.Modified;
				DB.SaveChanges();
				DB2.SaveChanges();

				if (promoFileRemove != null)
				{
					//    DB.MediaFiles.Remove(PromoFileRemove);
					//    DB.SaveChanges();
					//    для лоигрования действий решил не удалять файл для возможности восстановления промоакции
				}
				promoDb.MediaFile = file;
			}
			else if (model.PromotionFileId != null) {
				promoDb.MediaFile = DB2.MediaFiles.Find(model.PromotionFileId.Value);
			}
			DB.SaveChanges();
			return promoDb.Id;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private long SaveNewPromotion(PromotionUi model)
		{
			var file = SaveFile(model.File);
			var regionMask = model.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);
			if (file == null)
			{
				if (model.PromotionFileId != null)
					file = DB2.MediaFiles.Find(model.PromotionFileId.Value);
			}

			var promotionToDataBase = new Promotion()
			{
				Name = model.Name,
				Annotation = model.Annotation,
				Begin = Convert.ToDateTime(model.Begin),
				End = Convert.ToDateTime(model.End),
				UpdateTime = DateTime.Now,
				AgencyDisabled = false,
				Enabled = true,
				Status = (byte)PromotionStatus.New,
				ProducerId = producerId,
				Author = DB2.Users.Find(CurrentUser.Id),
				MediaFile = file,
				RegionMask = regionMask,
				PromoFile = ""
			};
			DB2.Promotions.Add(promotionToDataBase);
			DB2.SaveChanges();
			DB.SaveChanges(CurrentUser, "Добавление промоакции");

			foreach (var item in model.DrugList)
			{
				var promotionToDrug = new PromotionToDrug() {
					DrugId = Convert.ToInt64(item),
					PromotionId = promotionToDataBase.Id
				};
				DB2.PromotionToDrugs.Add(promotionToDrug);
			}
			DB.SaveChanges(CurrentUser, "Добавление лекарств к промоакции");

			if (model.SuppierRegions.Contains("0")) {
				model.SuppierRegions = h.GetSupplierList(model.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				promotionToDataBase.AllSuppliers = true;
			}
			else
				promotionToDataBase.AllSuppliers = false;

			foreach (var item in model.SuppierRegions)
			{
				var X = new PromotionsToSupplier() { PromotionId = promotionToDataBase.Id, SupplierId = Convert.ToInt64(item) };
				DB2.PromotionsToSuppliers.Add(X);
			}
			DB.SaveChanges(CurrentUser, "Добавление поставщиков к промоакции");
			DB2.SaveChanges();
			return promotionToDataBase.Id;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		private MediaFile SaveFile(HttpPostedFileBase src)
		{
			if (src == null)
				return null;

			var file = new MediaFile(src.FileName);
			var ms = new MemoryStream();
			src.InputStream.CopyTo(ms);

			file.ImageFile = ms.ToArray();
			file.ImageType = src.ContentType;
			file.ImageSize = ms.Length.ToString();
			file.EntityType = (int)EntityType.Promotion;
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
			var promoAction = DB2.Promotions.SingleOrDefault(x => x.Id == Id);
			if (promoAction == null)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}
			if (promoAction.ProducerId != producerId)
			{
				ErrorMessage("У вас нет прав для изменения данной акции");
				return RedirectToAction("Index");
			}

			promoAction.Enabled = Enabled;
			DB.SaveChanges(CurrentUser, "Изменения статуса акции");
			DB2.SaveChanges();

			var message = promoAction.GetStatus().DisplayName();
			SuccessMessage($"Новый статус: {message}");
			return RedirectToAction("Index", new { Id = Id.ToString() });
		}

		public FileResult GetFile(int Id)
		{
			var Image = DB.MediaFiles.Find(Id);
			return File(Image.ImageFile, Image.ImageType, Image.ImageName);
		}

		public ActionResult DeletePromo(long? Id)
		{
			var promoAction = DB2.Promotions.SingleOrDefault(x => x.Id == Id);
			if (promoAction == null)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}
			if (promoAction.ProducerId != producerId)
			{
				ErrorMessage("У вас нет прав для удаления данной акции");
				return RedirectToAction("Index");
			}

			if (promoAction.MediaFile != null)
			{
				var fileId = promoAction.MediaFile.Id;
				promoAction.MediaFile = null;
				DB.Entry(promoAction).State = EntityState.Modified;
				DB.SaveChanges(CurrentUser, "Удаление акции");
				DB.MediaFiles.Remove(DB.MediaFiles.First(x => x.Id == fileId));
				DB.SaveChanges(CurrentUser);
			}

			var suppliers = DB2.PromotionsToSuppliers.Where(x => x.PromotionId == promoAction.Id).ToList();
			foreach (var item in suppliers)
			{
				DB2.PromotionsToSuppliers.Remove(item);
				DB.SaveChanges(CurrentUser);
			}

			var drugList = DB2.PromotionToDrugs.Where(x => x.PromotionId == promoAction.Id).ToList();
			foreach (var item in drugList)
			{
				DB2.PromotionToDrugs.Remove(item);
				DB.SaveChanges(CurrentUser);
			}
			DB2.Promotions.Remove(promoAction);
			DB.SaveChanges(CurrentUser, "Удаление акции");
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