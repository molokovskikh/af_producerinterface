using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using Quartz;
using Quartz.Impl;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Models;
using System.Configuration;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using ProducerInterface.Controllers;
using ProducerInterfaceCommon.ViewModel.Interface.Promotion;
namespace ProducerInterface.Controllers
{
	public class PromotionController : MasterBaseController
	{

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			if (HttpContext != null && CurrentUser != null)
			{
				h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id, HttpContext);
			}
		}

		private ProducerInterfaceCommon.Heap.NamesHelper h;

		public ActionResult Index(string Id = null)
		{

			IEnumerable<promotions> list = cntx_.promotions.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).ToList();


			if (Id != null)
			{
				ViewBag.OpenPromotionId = Id;
			}

			ViewBag.GrugList = h.GetCatalogListPromotion();
			ViewBag.RegionList = h.GetRegionList();
			var SupplierList = h.GetSupplierList(new List<decimal>() { 0 });
			SupplierList.Add(new OptionElement() { Text = "Все поставщики в выбранных регионах", Value = "0" });
			ViewBag.SupplierList = SupplierList;

			foreach (var list_item in list)
			{
				list_item.RegionList = h.GetPromotionRegions((ulong)list_item.RegionMask);
			}

			return View(list);
		}

		[HttpGet]
		public ActionResult Edit(long IdKey = 0, bool Copy = false)
		{
			string ID_ = IdKey.ToString();
			if (Copy)
			{
				ID_ += ", true";
			}
			ViewBag.PromotionId = ID_;
			return View();
		}

		[HttpPost]
		public JsonResult EditGetPromotion(string IdKey = "0")
		{
			PromotionEdit ViewModel = new PromotionEdit();

			long ID = Convert.ToInt64(IdKey.ToString().Split(new Char[] { ',' }).First());
			if (IdKey.Contains("true"))
			{
				ID = 0;
			}

			var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);


			ViewModel.DrugCatalogList = h.GetCatalogList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
			ViewModel.RegionGlobalList = h.GetRegionList().ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();

			if (IdKey == "0")
			{
				// возвращаем новую промо акцию
				ViewModel.Id = 0;
				ViewModel.Title = "Новая промоакция";
				ViewModel.Name = "";
				ViewModel.Annotation = "";
				ViewModel.Begin = DateTime.Now.ToString("dd.MM.yyyy");
				ViewModel.End = DateTime.Now.ToString("dd.MM.yyyy");
				ViewModel.SuppierRegionsList = h.GetSupplierList(new List<decimal>() { 0 }).Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
				ViewModel.RegionList = h.GetPromotionRegions(Convert.ToUInt64(0)).ToList().Select(x => x.ToString()).ToList();
				ViewModel.RegionGlobalList = h.GetRegionList().ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();
			}
			else
			{
				var ID_Promo = Convert.ToInt64(IdKey.Split(new Char[] { ',' }).First());

				var ChangePromo = cntx_.promotions.Find(ID_Promo);
				ViewModel.Id = ID;
				ViewModel.SuppierRegions = ChangePromo.PromotionsToSupplier.ToList().Select(x => (string)x.SupplierId.ToString()).ToList();

				ViewModel.SuppierRegionsList = h.GetSupplierList(ViewModel.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();				
				ViewModel.Name = ChangePromo.Name;

				if (ChangePromo.AllSuppliers != null && ChangePromo.AllSuppliers != false)
				{
					ViewModel.SuppierRegions = new List<string>() { "0" };
				}

				if (ID == 0)
				{
					ViewModel.Name += " Копия";
					ViewModel.Title = "Создание промоакции: копия из " + ChangePromo.Name;
				}
				else
				{
					ViewModel.Title = "Редактирование промоакции: " + ChangePromo.Name;
				}
				if (ChangePromo.PromoFileId != null)
				{
					ViewModel.PromotionFileId = (long)ChangePromo.PromoFileId;
					ViewModel.PromotionFileName = ChangePromo.MediaFiles?.ImageName;
				}
				ViewModel.DrugList = ChangePromo.promotionToDrug.ToList().Select(x => x.DrugId.ToString()).ToList();
				if (ChangePromo.Begin < DateTime.Now)
				{
					ViewModel.Begin = DateTime.Now.ToString("dd.MM.yyyy");
				}
				else
				{
					ViewModel.Begin = ChangePromo.Begin.Value.ToString("dd.MM.yyyy");
				}

				if (ChangePromo.End < DateTime.Now)
				{
					ViewModel.End = DateTime.Now.ToString("dd.MM.yyyy");
				}
				else
				{
					ViewModel.End = ChangePromo.End.Value.ToString("dd.MM.yyyy");
				}

				ViewModel.RegionList = h.GetPromotionRegions(Convert.ToUInt64(ChangePromo.RegionMask)).ToList().Select(x => x.ToString()).ToList();
				ViewModel.SuppierRegionsList = h.GetSupplierList(ViewModel.RegionList.Select(x => (decimal)Convert.ToUInt64(x)).ToList()).ToList()
						.Select(x => new TextValue { Text = x.Text, Value = x.Value }).ToList();

				ViewModel.PromotionFileUrl = Url.Action("GetFile", new { id = ChangePromo.MediaFiles?.Id });
				ViewModel.Annotation = ChangePromo.Annotation;
			}

			ViewModel.SuppierRegionsList.Add(new TextValue() { Text = "Все поставщики из выбранных регионов", Value = "0" });

			var JsonModel = Json(ViewModel);

			return Json(ViewModel, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult GetUpdateSupplierList(List<decimal> Id)
		{

			if (Id == null || Id.Count() == 0)
			{
				return Json(new List<TextValue>(), JsonRequestBehavior.AllowGet);
			}

			var SupplierListOptionElements = h.GetSupplierList(Id).ToList();

			var SupplierList = SupplierListOptionElements
					.Select(x => new TextValue { Text = x.Text, Value = x.Value })
					.ToList();

			SupplierList.Add(new TextValue() { Text = "Все поставщики из выбранных регионов", Value = "0" });
			return Json(SupplierList, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult EditSave(PromotionEdit PE)
		{

			if (PE.Id == 0)
			{
				// добавить
				long IdNewPromo = SaveNewPromotion(PE);
				SuccessMessage("Промо акция добавлена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index", new { Id = IdNewPromo });

			}
			else
			{
				long IdNewPromo = ChangePromotion(PE);
				SuccessMessage("Промо акция изменена и отправлен запрос на её подтверждение");
				return RedirectToAction("Index", new { Id = IdNewPromo });
			}

		}

		private long ChangePromotion(PromotionEdit newPromo)
		{
			var PromoDB = cntx_.promotions.Find(newPromo.Id);

			var promotion_to_Drug = PromoDB.promotionToDrug.ToList();

			foreach (var DrugItem in promotion_to_Drug)
			{
				bool DrugExsist = newPromo.DrugList.Any(x => Convert.ToInt64(x) == DrugItem.DrugId);

				if (!DrugExsist)
				{
					cntx_.promotionToDrug.Remove(DrugItem);
				}

			}

			cntx_.SaveChanges(CurrentUser, "Препарат удален из акции");

			foreach (var DrugItem in newPromo.DrugList)
			{
				bool DrugExsist = PromoDB.promotionToDrug.Any(x => x.DrugId == Convert.ToInt64(DrugItem));

				if (!DrugExsist)
				{
					promotionToDrug newAddDrug = new promotionToDrug()
					{
						DrugId = Convert.ToInt64(DrugItem),
						PromotionId = PromoDB.Id
					};

					cntx_.promotionToDrug.Add(newAddDrug);

				}
			}

			cntx_.SaveChanges(CurrentUser, "Препарат добавлен в акцию");

			var Promo_PromotionsToSupplier = PromoDB.PromotionsToSupplier.ToList();

			foreach (var SupplierItem in Promo_PromotionsToSupplier)
			{
				bool SupllierExsist = newPromo.SuppierRegions.Any(x => (ulong)Convert.ToInt64(x) == (ulong)SupplierItem.SupplierId);

				if (!SupllierExsist)
				{
					cntx_.PromotionsToSupplier.Remove(SupplierItem);
				}
			}

			cntx_.SaveChanges(CurrentUser, "Поставщик удален из акции");

			if (newPromo.SuppierRegions.Contains("0"))
			{
				newPromo.SuppierRegions = h.GetSupplierList(newPromo.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				PromoDB.AllSuppliers = true;
			}
			else
			{
				PromoDB.AllSuppliers = false;
			}
			foreach (var SupplierItem in newPromo.SuppierRegions)
			{
				bool SupllierExsist = PromoDB.PromotionsToSupplier.Any(x => (ulong)x.SupplierId == (ulong)Convert.ToInt64(SupplierItem));

				if (!SupllierExsist)
				{
					PromotionsToSupplier AddNew = new PromotionsToSupplier()
					{ SupplierId = (long)Convert.ToInt64(SupplierItem), PromotionId = PromoDB.Id };
					PromoDB.PromotionsToSupplier.Add(AddNew);
				}
			}

			cntx_.SaveChanges(CurrentUser, "Поставщик добавлен в акцию");

			ulong regionMask = 0;

			if (newPromo.RegionList.Count() == 1)
			{
				regionMask = (ulong)Convert.ToInt64(newPromo.RegionList.First());
			}
			else
			{
				regionMask = newPromo.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);
			}
			PromoDB.RegionMask = regionMask;
			PromoDB.Name = newPromo.Name;
			PromoDB.Annotation = newPromo.Annotation;

			PromoDB.Begin = Convert.ToDateTime(newPromo.Begin);
			PromoDB.End = Convert.ToDateTime(newPromo.End);

			PromoDB.AgencyDisabled = false;
			PromoDB.Enabled = true;
			PromoDB.Status = false;

			PromoDB.ProducerUserId = CurrentUser.Id;

			cntx_.Entry(PromoDB).State = EntityState.Modified;
			cntx_.SaveChanges();

			int? FileID = SaveFile(newPromo.File);

			if (FileID != null)
			{
				var PromoFileRemove = cntx_.MediaFiles.Find(PromoDB.PromoFileId);

				PromoDB.PromoFileId = null;
				PromoDB.PromoFile = "";

				cntx_.Entry(PromoDB).State = EntityState.Modified;
				cntx_.SaveChanges();

				if (PromoFileRemove != null)
				{
					//    cntx_.MediaFiles.Remove(PromoFileRemove);
					//    cntx_.SaveChanges();

					//    для лоигрования действий решил не удалять файл для возможности восстановления промоакции
				}

				PromoDB.PromoFileId = FileID;
			}
			else
			{
				if (newPromo.PromotionFileId != null)
				{
					FileID = (int)newPromo.PromotionFileId;
					PromoDB.PromoFileId = FileID;
				}
			}

			cntx_.SaveChanges();

			return PromoDB.Id;
		}

		private long SaveNewPromotion(PromotionEdit newPromo)
		{

			int? FileId = SaveFile(newPromo.File);
			var regionMask = newPromo.RegionList.Select(x => (ulong)Convert.ToInt64(x)).Aggregate((y, z) => y | z);

			if (FileId == null)
			{
				if (newPromo.PromotionFileId != null)
				{
					FileId = (int)newPromo.PromotionFileId;
				}
			}

			promotions promotionToDataBase = new promotions()
			{
				Name = newPromo.Name,
				Annotation = newPromo.Annotation,

				Begin = Convert.ToDateTime(newPromo.Begin),
				End = Convert.ToDateTime(newPromo.End),
				UpdateTime = DateTime.Now,

				AgencyDisabled = false,
				Enabled = true,
				Status = false,

				ProducerId = (long)CurrentUser.AccountCompany.ProducerId,
				ProducerUserId = CurrentUser.Id,

				PromoFileId = FileId,
				RegionMask = regionMask,
				PromoFile = ""
			};

			cntx_.promotions.Add(promotionToDataBase);
			//    cntx_.SaveChanges();
			cntx_.SaveChanges(CurrentUser, "Добавление промоакции");

			foreach (var DrugItem in newPromo.DrugList)
			{
				var X = new promotionToDrug() { DrugId = Convert.ToInt64(DrugItem), PromotionId = promotionToDataBase.Id };
				cntx_.promotionToDrug.Add(X);
			}
			//    cntx_.SaveChanges();
			cntx_.SaveChanges(CurrentUser, "Добавление лекарств к промоакции");

			if (newPromo.SuppierRegions.Contains("0")) {
				newPromo.SuppierRegions = h.GetSupplierList(newPromo.SuppierRegions.ToList().Select(x => (ulong)Convert.ToInt64(x)).ToList()).ToList().Select(x => x.Value).ToList();
				promotionToDataBase.AllSuppliers = true;
			}
			else {
				promotionToDataBase.AllSuppliers = false;
			}

			foreach (var SupplierItem in newPromo.SuppierRegions)
			{
				var X = new PromotionsToSupplier() { PromotionId = promotionToDataBase.Id, SupplierId = (long)Convert.ToInt64(SupplierItem) };
				cntx_.PromotionsToSupplier.Add(X);
			}

			//     cntx_.SaveChanges();
			cntx_.SaveChanges(CurrentUser, "Добавление поставщиков к промоакции");

			return promotionToDataBase.Id;
		}

		private int? SaveFile(HttpPostedFileBase _file)
		{
			if (_file == null)
			{
				return null;
			}

			var PromoFile = new MediaFiles();

			MemoryStream MS = new MemoryStream();
			_file.InputStream.CopyTo(MS);

			PromoFile.ImageFile = MS.ToArray();
			PromoFile.ImageName = _file.FileName.Split(new Char[] { '\\' }).Last();
			PromoFile.ImageType = _file.ContentType;
			PromoFile.ImageSize = MS.Length.ToString();
			PromoFile.EntityType = (int)EntityType.Promotion;
			cntx_.MediaFiles.Add(PromoFile);
			cntx_.SaveChanges();

			return PromoFile.Id;
		}

		[HttpGet]
		public ActionResult Publication(long Id, bool Enabled)
		{

			var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).FirstOrDefault();

			if (PromoAction != null && PromoAction.ProducerId == CurrentUser.AccountCompany.ProducerId)
			{
				PromoAction.Enabled = Enabled;
				cntx_.Entry(PromoAction).State = EntityState.Modified;
				cntx_.SaveChanges(CurrentUser, "Изменения статуса акции");
			}

			if (Enabled)
			{
				if (System.DateTime.Now > PromoAction.Begin && System.DateTime.Now < PromoAction.End)
				{
					SuccessMessage("акция публикуется");
				}
				else
				{
					SuccessMessage("при наступлении даты начала акция будет публиковатся");
				}
			}
			else
			{
				SuccessMessage("публикация акции отключена");
			}
			return RedirectToAction("Index", new { Id = Id.ToString() });
		}

		public FileResult GetFile(int Id)
		{
			var Image = cntx_.MediaFiles.Find(Id);
			return File(Image.ImageFile, Image.ImageType, Image.ImageName);//      fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
		}

		public ActionResult DeletePromo(long? Id)
		{
			var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).FirstOrDefault();

			if (PromoAction == null && Id == 0)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}
			if (PromoAction.ProducerId != CurrentUser.AccountCompany.ProducerId)
			{
				ErrorMessage("У вас нет прав для удаления данной акции");
				return RedirectToAction("Index");
			}

			if (PromoAction.PromoFileId != null)
			{
				var IdFile = PromoAction.PromoFileId;
				PromoAction.PromoFileId = null;
				cntx_.Entry(PromoAction).State = EntityState.Modified;
				cntx_.SaveChanges(CurrentUser, "Удаление акции");

				cntx_.MediaFiles.Remove(cntx_.MediaFiles.Where(xxx => xxx.Id == IdFile).First());
				cntx_.SaveChanges(CurrentUser);
			}

			var Suppliers = cntx_.PromotionsToSupplier.Where(x => x.PromotionId == PromoAction.Id).ToList();
			foreach (var SupItem in Suppliers)
			{
				cntx_.PromotionsToSupplier.Remove(SupItem);
				cntx_.SaveChanges(CurrentUser);
			}

			var DrugList = cntx_.promotionToDrug.Where(x => x.PromotionId == PromoAction.Id).ToList();

			foreach (var DrugItem in DrugList)
			{
				cntx_.promotionToDrug.Remove(DrugItem);
				cntx_.SaveChanges(CurrentUser);
			}

			cntx_.promotions.Remove(PromoAction);
			cntx_.SaveChanges(CurrentUser, "Удаление акции");

			SuccessMessage("Акция удалена");
			return RedirectToAction("Index");
		}

		public JsonResult GetSupplierJson(List<decimal> RegionList, List<long> SuppierRegions)
		{
			var h = new NamesHelper(cntx_, CurrentUser.Id);

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