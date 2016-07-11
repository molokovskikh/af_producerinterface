using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.Data.Entity;
using System.IO;
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
				h = new NamesHelper(DB, CurrentUser.Id, HttpContext);
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
			var promoList = DB.promotions.Where(x => x.ProducerId == producerId).ToList();
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
					PromotionFileId = item.PromoFileId,
					PromotionFileName = item.MediaFiles?.ImageName,
					AllSuppliers = item.AllSuppliers,
					FakeStatus = GetStatus(item),
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

		private PromotionFakeStatus GetStatus(promotions model)
		{
			// отклонена админом
			if (model.Status == (byte)PromotionStatus.Rejected)
				return PromotionFakeStatus.Rejected;

			// отключена пользователем
			if (!model.Enabled)
				return PromotionFakeStatus.Disabled;

			// ожидает согласования
			if (model.Enabled && model.Status == (byte)PromotionStatus.New)
				return PromotionFakeStatus.NotConfirmed;

			// не отклонена, согласована, не началась
			if (model.Enabled && model.Status == (byte)PromotionStatus.Confirmed && model.Begin > DateTime.Now)
				return PromotionFakeStatus.ConfirmedNotBegin;

			// не отклонена, согласована, просрочена
			if (model.Enabled && model.Status == (byte)PromotionStatus.Confirmed && model.End < DateTime.Now)
				return PromotionFakeStatus.ConfirmedEnded;

			// активная
			if (model.Enabled && model.Status == (byte)PromotionStatus.Confirmed && model.Begin < DateTime.Now && model.End > DateTime.Now)
				return PromotionFakeStatus.Active;

			throw new NotSupportedException("Неизвестный статус");
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="IdKey"></param>
		/// <param name="Copy"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Edit(long IdKey = 0, bool Copy = false)
		{
			var id = IdKey.ToString();
			if (Copy)
				id += ", true";
			ViewBag.PromotionId = id;
			return View();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="IdKey"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult EditGetPromotion(string IdKey = "0")
		{
			var model = new PromotionUi();
			var id = Convert.ToInt64(IdKey.Split(new Char[] { ',' }).First());
			if (IdKey.Contains("true"))
				id = 0;

			var h = new NamesHelper(DB, CurrentUser.Id); // ??
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
				var dBmodel = DB.promotions.Find(idPromo);
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

				if (dBmodel.PromoFileId != null)
				{
					model.PromotionFileId = (long)dBmodel.PromoFileId;
					model.PromotionFileName = dBmodel.MediaFiles?.ImageName;
				}
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

				model.PromotionFileUrl = Url.Action("GetFile", new { id = dBmodel.MediaFiles?.Id });
				model.Annotation = dBmodel.Annotation;
			}

			model.SuppierRegionsList.Add(new TextValue() { Text = "Все поставщики из выбранных регионов", Value = "0" });

			var JsonModel = Json(model);

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
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

		/// <summary>
		///
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
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

		/// <summary>
		///
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private long ChangePromotion(PromotionUi model)
		{
			var promoDb = DB.promotions.Find(model.Id);
			var promotionToDrug = promoDb.promotionToDrug.ToList();
			foreach (var item in promotionToDrug)
			{
				var drugExsist = model.DrugList.Any(x => Convert.ToInt64(x) == item.DrugId);
				if (!drugExsist)
					DB.promotionToDrug.Remove(item);
			}
			DB.SaveChanges(CurrentUser, "Препарат удален из акции");
			foreach (var item in model.DrugList)
			{
				var drugExsist = promoDb.promotionToDrug.Any(x => x.DrugId == Convert.ToInt64(item));
				if (!drugExsist)
				{
					promotionToDrug newAddDrug = new promotionToDrug()
					{
						DrugId = Convert.ToInt64(item),
						PromotionId = promoDb.Id
					};
					DB.promotionToDrug.Add(newAddDrug);
				}
			}
			DB.SaveChanges(CurrentUser, "Препарат добавлен в акцию");

			var promoPromotionsToSupplier = promoDb.PromotionsToSupplier.ToList();
			foreach (var item in promoPromotionsToSupplier)
			{
				var supllierExsist = model.SuppierRegions.Any(x => (ulong)Convert.ToInt64(x) == (ulong)item.SupplierId);
				if (!supllierExsist)
					DB.PromotionsToSupplier.Remove(item);
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
			promoDb.ProducerUserId = CurrentUser.Id;
			DB.Entry(promoDb).State = EntityState.Modified;
			DB.SaveChanges();

			int? saveFile = SaveFile(model.File);
			if (saveFile != null)
			{
				var promoFileRemove = DB.MediaFiles.Find(promoDb.PromoFileId);
				promoDb.PromoFileId = null;
				promoDb.PromoFile = "";
				DB.Entry(promoDb).State = EntityState.Modified;
				DB.SaveChanges();

				if (promoFileRemove != null)
				{
					//    DB.MediaFiles.Remove(PromoFileRemove);
					//    DB.SaveChanges();
					//    для лоигрования действий решил не удалять файл для возможности восстановления промоакции
				}
				promoDb.PromoFileId = saveFile;
			}
			else
			{
				if (model.PromotionFileId != null)
				{
					saveFile = (int)model.PromotionFileId;
					promoDb.PromoFileId = saveFile;
				}
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
			int? saveFile = SaveFile(model.File);
			var regionMask = model.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);
			if (saveFile == null)
			{
				if (model.PromotionFileId != null)
					saveFile = (int)model.PromotionFileId;
			}

			var promotionToDataBase = new promotions()
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
				ProducerUserId = CurrentUser.Id,
				PromoFileId = saveFile,
				RegionMask = regionMask,
				PromoFile = ""
			};
			DB.promotions.Add(promotionToDataBase);
			DB.SaveChanges(CurrentUser, "Добавление промоакции");

			foreach (var item in model.DrugList)
			{
				var promotionToDrug = new promotionToDrug() {
					DrugId = Convert.ToInt64(item),
					PromotionId = promotionToDataBase.Id
				};
				DB.promotionToDrug.Add(promotionToDrug);
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
				DB.PromotionsToSupplier.Add(X);
			}
			DB.SaveChanges(CurrentUser, "Добавление поставщиков к промоакции");
			return promotionToDataBase.Id;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="_file"></param>
		/// <returns></returns>
		private int? SaveFile(HttpPostedFileBase _file)
		{
			if (_file == null)
				return null;

			var promoFile = new MediaFiles();
			var ms = new MemoryStream();
			_file.InputStream.CopyTo(ms);

			promoFile.ImageFile = ms.ToArray();
			promoFile.ImageName = _file.FileName.Split(new Char[] { '\\' }).Last();
			promoFile.ImageType = _file.ContentType;
			promoFile.ImageSize = ms.Length.ToString();
			promoFile.EntityType = (int)EntityType.Promotion;
			DB.MediaFiles.Add(promoFile);
			DB.SaveChanges();
			return promoFile.Id;
		}

		/// <summary>
		/// Запуск-остановка публикации акции
		/// </summary>
		/// <param name="Id">идентификатор акции</param>
		/// <param name="Enabled">статус</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Publication(long Id, bool Enabled)
		{
			var promoAction = DB.promotions.SingleOrDefault(x => x.Id == Id);
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

			var message = GetStatus(promoAction).DisplayName();
			SuccessMessage($"Новый статус: {message}");
			return RedirectToAction("Index", new { Id = Id.ToString() });
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public FileResult GetFile(int Id)
		{
			var Image = DB.MediaFiles.Find(Id);
			return File(Image.ImageFile, Image.ImageType, Image.ImageName);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public ActionResult DeletePromo(long? Id)
		{
			var promoAction = DB.promotions.SingleOrDefault(x => x.Id == Id);
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

			if (promoAction.PromoFileId != null)
			{
				var fileId = promoAction.PromoFileId;
				promoAction.PromoFileId = null;
				DB.Entry(promoAction).State = EntityState.Modified;
				DB.SaveChanges(CurrentUser, "Удаление акции");
				DB.MediaFiles.Remove(DB.MediaFiles.First(x => x.Id == fileId));
				DB.SaveChanges(CurrentUser);
			}

			var suppliers = DB.PromotionsToSupplier.Where(x => x.PromotionId == promoAction.Id).ToList();
			foreach (var item in suppliers)
			{
				DB.PromotionsToSupplier.Remove(item);
				DB.SaveChanges(CurrentUser);
			}

			var drugList = DB.promotionToDrug.Where(x => x.PromotionId == promoAction.Id).ToList();
			foreach (var item in drugList)
			{
				DB.promotionToDrug.Remove(item);
				DB.SaveChanges(CurrentUser);
			}
			DB.promotions.Remove(promoAction);
			DB.SaveChanges(CurrentUser, "Удаление акции");
			SuccessMessage("Акция удалена");
			return RedirectToAction("Index");
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="RegionList"></param>
		/// <param name="SuppierRegions"></param>
		/// <returns></returns>
		public JsonResult GetSupplierJson(List<decimal> RegionList, List<long> SuppierRegions)
		{
			var h = new NamesHelper(DB, CurrentUser.Id);

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