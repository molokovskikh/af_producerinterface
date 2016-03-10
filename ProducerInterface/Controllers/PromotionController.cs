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

namespace ProducerInterface.Controllers
{
	public class PromotionController : MasterBaseController
	{

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
		public ActionResult Index(string Id = null)
		{
			//var list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId && xxx.Status).ToList();
			IEnumerable<promotions> list = cntx_.promotions.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).ToList();
			var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);

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
		public ActionResult Manage(long? id)
		{
			PromotionValidation ViewPromotion = new PromotionValidation();

			//   var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
			ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
			ProducerInterfaceCommon.Heap.NamesHelper h;
			cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
			h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);

			ViewData["DrugList"] = h.GetCatalogList();
			ViewData["RegionList"] = h.GetRegionList();
            ViewData["SuppierRegions"] = h.GetSupplierList(new List<decimal>() { 0 });

			if (id.HasValue)
			{
				//ViewBag.CurrentPromotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
				// редактирование существующей
				ViewPromotion = cntx_.promotions.Where(xxx => xxx.Id == id).ToList().Select(xxx =>
				new PromotionValidation
				{
					Id = xxx.Id,
					Name = xxx.Name,
					Annotation = xxx.Annotation,
					Begin = xxx.Begin,
					End = xxx.End,
					Status = xxx.Status,
					PromotionFileId = xxx.PromoFileId,
					PromotionFileName = xxx.PromoFile,
					RegionList = h.GetPromotionRegions(Convert.ToUInt64(xxx.RegionMask)),
                    SuppierRegions = cntx_.PromotionsToSupplier.Where(z => z.PromotionId == xxx.Id).ToList().Select(v=>v.SupplierId).ToList()
				}).FirstOrDefault();

                var SuppierRegionsList = h.GetSupplierList(ViewPromotion.RegionList.Select(x => (decimal)x).ToList());

                if (ViewPromotion.SuppierRegions.Contains(0))
                {
                    SuppierRegionsList.Add(new OptionElement() { Value = "0", Text = "Все поставщики в выбранных регионах" });
                }
                ViewData["SuppierRegions"] = SuppierRegionsList;

                //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
                ViewPromotion.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == id).ToList().Select(xxx => xxx.DrugId).ToList();
			}
			else
			{

				ViewPromotion.RegionList = h.GetRegionList().Where(xxx => xxx.Text == "Все регионы").Select(xxx => (long)Convert.ToInt64(xxx.Value)).ToList();
                ViewData["SuppierRegions"] = h.GetSupplierList(new List<decimal>() { 0 });
                // Создание новой акции нужное значение уже присвоено
                //var CurrentPromotion = new promotions();
            }
			return View(ViewPromotion);
		}

		[HttpPost]
		public ActionResult Manage(PromotionValidation PromoAction)
		{

			if (!ModelState.IsValid)
			{
				ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
				ProducerInterfaceCommon.Heap.NamesHelper h;
				cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
				h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);
				ViewData["DrugList"] = h.GetCatalogList();
				ViewData["RegionList"] = h.GetRegionList();

                var regList = PromoAction.RegionList?.Select(x => (decimal)x).ToList();


                ViewData["SuppierRegions"] = h.GetSupplierList(regList);    
                return View(PromoAction);
			}

			var PromotionSave = new ProducerInterfaceCommon.ContextModels.promotions();
			bool PromotionNewOrOld = false; // старая акция или новая акция (true = старая / false = новая)

			if (PromoAction.Id != 0)
			{
				PromotionNewOrOld = true;
				PromotionSave = cntx_.promotions.Where(xxx => xxx.Id == PromoAction.Id).First();
				// получаем акцию из БД
			}

			bool validationChanges = (PromotionNewOrOld && ValidationChangesPromotion(PromotionSave, PromoAction));
			// возвращает false в случае, если что то в акции изменилось, или если это новая акция // true - старая акция и ничего не изменилось

			var regionMask = PromoAction.RegionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);

			bool RegionChanged = false;

			if (regionMask == (ulong)PromotionSave.RegionMask)
			{
				RegionChanged = true;
			}

			if (validationChanges && RegionChanged)
			{
				SuccessMessage("Акция не изменена");
				return RedirectToAction("Index");
			}


			if (PromoAction.File != null)
			{
				var PromoFile = new MediaFiles();

				MemoryStream MS = new MemoryStream();
				PromoAction.File.InputStream.CopyTo(MS);

				PromoFile.ImageFile = MS.ToArray();
				PromoFile.ImageName = PromoAction.File.FileName.Split(new Char[] { '\\' }).Last();
				PromoFile.ImageType = PromoAction.File.ContentType;
				PromoFile.ImageSize = MS.Length.ToString();
				PromoFile.EntityType = (int)EntityType.Promotion;
				cntx_.MediaFiles.Add(PromoFile);
				cntx_.SaveChanges();

				PromotionSave.PromoFileId = PromoFile.Id;
				PromotionSave.PromoFile = PromoFile.ImageName;
			}

			//    var regionList = PromoAction.RegionList;
			//    var regionMask = regionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);

			PromotionSave.RegionMask = PromoAction.RegionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);

			PromotionSave.Name = PromoAction.Name;
			PromotionSave.Status = false;
			PromotionSave.Enabled = true;
			PromotionSave.Annotation = PromoAction.Annotation;
			PromotionSave.Begin = PromoAction.Begin;
			PromotionSave.End = PromoAction.End;
			PromotionSave.ProducerId = (long)CurrentUser.AccountCompany.ProducerId;
            //PromotionSave.UpdateTime = DateTime.Now;

            var SullierOldList = cntx_.PromotionsToSupplier.Where(x => x.PromotionId == PromoAction.Id).ToList();

            // адаляем удаленных постовщиков из акции в БД
            foreach (var SupplierDelete in SullierOldList)
            {
                var SupplierExsist = PromoAction.SuppierRegions.Any(x => x == SupplierDelete.SupplierId);
                if (!SupplierExsist)
                {
                    cntx_.Entry(SupplierDelete).State = EntityState.Deleted;
                    cntx_.SaveChanges(CurrentUser, "удаление поставщиков из акции");
                }
            }
          
			var ListOldDrugInPromotion = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == PromoAction.Id).ToList();
            
			if (PromoAction.Id != 0)
			{
				foreach (var OneDrugItem in ListOldDrugInPromotion)
				{
					// проверяем осталось ли в моделе лекарство, которое пришло из БД
					bool DrugOstalsyz = PromoAction.DrugList.Any(xxx => xxx == OneDrugItem.DrugId);

					if (!DrugOstalsyz) // если нет в списке, удаляем из БД
					{
						cntx_.promotionToDrug.Remove(OneDrugItem);
					}
				}
				cntx_.Entry(PromotionSave).State = EntityState.Modified;
				cntx_.SaveChanges(CurrentUser); // сохраняем в БД удалённые лекарства
			}

			if (PromotionSave.Id == 0)
			{
				// PromotionSave.ProducerUserId = CurrentUser.Id;  в новую акцию добавляем Id пользователя
				PromotionSave.Account = CurrentUser;
				cntx_.Entry(PromotionSave).State = EntityState.Added;
				cntx_.SaveChanges(CurrentUser);
			}
            
			foreach (var GrugItem in PromoAction.DrugList)
			{

				bool OneDrugIf = ListOldDrugInPromotion.Any(xxx => xxx.DrugId == GrugItem);

				if (!OneDrugIf) // для данного лекарства нет записи в БД
				{
					var DrugInPromotion = new promotionToDrug() { DrugId = GrugItem, PromotionId = PromotionSave.Id };
					cntx_.promotionToDrug.Add(DrugInPromotion);
				}
				// привязка лекарств к акции           
			}
			cntx_.SaveChanges(CurrentUser);

            // добавляем выбранных поставщиков в БД

            foreach (var SupplierAdd in PromoAction.SuppierRegions)
            {
                bool ExsistSupplier = SullierOldList.Any(x => x.SupplierId == SupplierAdd);

                if (!ExsistSupplier)
                {
                    var NewSupplierItemToDB = new PromotionsToSupplier();
                    NewSupplierItemToDB.SupplierId = SupplierAdd;
                    NewSupplierItemToDB.promotions  = PromotionSave;

                    cntx_.Entry(NewSupplierItemToDB).State = EntityState.Added;
                    cntx_.SaveChanges(CurrentUser, "Привязка поставщиков к акции");

                }
            }

            // отправляем ссобщение пользователю об добавлении или изменении акции

            if (PromotionNewOrOld)
			{
				// старая изменена
				SuccessMessage("Акция изменена");
				ProducerInterfaceCommon.Heap.EmailSender.SendChangePromotion(cntx_, CurrentUser.Id, PromoAction.Id, CurrentUser.IP);
			}
			else
			{
				// новая акция добавлена
				SuccessMessage("Акция добавлена");
				ProducerInterfaceCommon.Heap.EmailSender.SendNewPromotion(cntx_, PromotionSave.ProducerUserId, PromotionSave.Id, CurrentUser.IP);
			}

			return RedirectToAction("Index", new { Id = PromotionSave.Id.ToString() });
		}

		private bool ValidationChangesPromotion(ProducerInterfaceCommon.ContextModels.promotions OldPromotion, PromotionValidation NewPromotion)
		{
			if (NewPromotion.Id == 0)
			{
				return false;
			}

			bool Name = OldPromotion.Name.Equals(NewPromotion.Name);
			bool Annotation = OldPromotion.Annotation.Equals(NewPromotion.Annotation);
			bool DataBegin = OldPromotion.Begin.Value.Equals(NewPromotion.Begin.Value);
			bool DataEnd = OldPromotion.End.Value.Equals(NewPromotion.End.Value);

			List<long> OldDrugList = OldPromotion.promotionToDrug.ToList().Select(xxx => xxx.DrugId).ToList();

			bool ListDrugsInPromotion = OldDrugList.Any(x => !NewPromotion.DrugList.Contains(x));

			bool AddFile = true;

			if (NewPromotion.File != null)
			{
				AddFile = false;
			}

            var SupplierListLong = OldPromotion.PromotionsToSupplier.ToList().Select(x => x.SupplierId).ToList();
            bool ChangeSupplierList = SupplierListLong.Any(x => NewPromotion.SuppierRegions.Contains(x));
            
            return (Name && Annotation && DataBegin && DataEnd && !ListDrugsInPromotion && AddFile && ChangeSupplierList);
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

		public ActionResult CopyPaste(long? Id)
		{

			if (Id == null && Id == 0)
			{
				return RedirectToAction("Index");
			}

			var ModelPromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).FirstOrDefault();

			if (ModelPromoAction == null && ModelPromoAction.Id == 0)
			{
				ErrorMessage("Акция не найдена");
				return RedirectToAction("Index");
			}

			if (ModelPromoAction.ProducerId != CurrentUser.AccountCompany.ProducerId)
			{
				ErrorMessage("У вас нет прав копировать акцию другого производителя");
				return RedirectToAction("Index");
			}

			ModelPromoAction.Name += " Копия!";

			ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
			ProducerInterfaceCommon.Heap.NamesHelper h;
			cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
			h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);

			var ModelView = new PromotionValidation
			{
				Name = ModelPromoAction.Name,
				Annotation = ModelPromoAction.Annotation,
				Begin = ModelPromoAction.Begin,
				End = ModelPromoAction.End,
				Status = ModelPromoAction.Status,
				RegionList = h.GetPromotionRegions(Convert.ToUInt64(ModelPromoAction.RegionMask))
			};

			ModelPromoAction = null;

			ViewData["DrugList"] = h.GetCatalogList();
			ViewData["RegionList"] = h.GetRegionList();

			ModelView.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == Id).ToList().Select(xxx => xxx.DrugId).ToList();

			return View("Manage", ModelView);
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
				cntx_.promotions.Remove(PromoAction);
				cntx_.SaveChanges(CurrentUser, "Удаление акции");
			}

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