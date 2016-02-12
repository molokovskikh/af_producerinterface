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
            if (id.HasValue)
            {
                //ViewBag.CurrentPromotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
                // редактирование существующей
                ViewPromotion = cntx_.promotions.Where(xxx => xxx.Id == id).ToList().Select(xxx =>
                new PromotionValidation
                { Id = xxx.Id,
                    Name = xxx.Name,
                    Annotation = xxx.Annotation,
                    Begin = xxx.Begin,
                    End = xxx.End, Status = xxx.Status,
                    PromotionFileId = xxx.PromoFileId,
                    PromotionFileName = xxx.PromoFile,
                    RegionList = h.GetPromotionRegions(Convert.ToUInt64(xxx.RegionMask))
                }).FirstOrDefault();
                              
                //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
                ViewPromotion.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == id).ToList().Select(xxx => xxx.DrugId).ToList();
            }
            else {

                ViewPromotion.RegionList = h.GetRegionList().Where(xxx => xxx.Text == "Все регионы").Select(xxx => (long)Convert.ToInt64(xxx.Value)).ToList();
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
                var PromoFile = new promotionsimage();

                MemoryStream MS = new MemoryStream();
                PromoAction.File.InputStream.CopyTo(MS);

                PromoFile.ImageFile = MS.ToArray();
                PromoFile.ImageName = PromoAction.File.FileName.Split(new Char[] { '\\' }).Last();
                PromoFile.ImageType = PromoAction.File.ContentType;
                PromoFile.ImageSize = MS.Length.ToString();

                cntx_.promotionsimage.Add(PromoFile);
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
        //    PromotionSave.UpdateTime = DateTime.Now;

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

            if (PromotionSave.Id == 0) {
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
           
        private bool ValidationChangesPromotion(ProducerInterfaceCommon.ContextModels.promotions OldPromotion,PromotionValidation NewPromotion)
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
            
            return (Name && Annotation && DataBegin && DataEnd && !ListDrugsInPromotion && AddFile);
        }

        [HttpGet]
        public ActionResult Publication(long Id, bool Enabled)
        {

            var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).FirstOrDefault();

            if (PromoAction != null && PromoAction.ProducerId == CurrentUser.AccountCompany.ProducerId)
            {
                PromoAction.Enabled = Enabled;
                cntx_.Entry(PromoAction).State = EntityState.Modified;
                cntx_.SaveChanges(CurrentUser, "Изменения статуса промо-акции");
            }

            if (Enabled)
            {
                if (System.DateTime.Now > PromoAction.Begin && System.DateTime.Now < PromoAction.End)
                {
                    SuccessMessage("промо-акция публикуется");
                }
                else
                {
                    SuccessMessage("при наступлении даты начала промо-акция будет публиковатся");
                }
            }
            else
            {
                SuccessMessage("публикация промо-акции отключена");
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
                ErrorMessage("У вас нет прав копировать промо-акцию другого производителя");
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
            var Image = cntx_.promotionsimage.Find(Id);
            return File(Image.ImageFile, Image.ImageType, Image.ImageName);//      fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult DeletePromo(long? Id)
        {
            var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).FirstOrDefault();

            if (PromoAction == null && Id == 0)
            {
                ErrorMessage("Промо-Акция не найдена");
                return RedirectToAction("Index");
            }
            if (PromoAction.ProducerId != CurrentUser.AccountCompany.ProducerId)
            {
                ErrorMessage("У вас нет прав для удаления данной промо-акции");
                return RedirectToAction("Index");
            }

            if (PromoAction.PromoFileId != null)
            {
                var IdFile = PromoAction.PromoFileId;
                PromoAction.PromoFileId = null;
                cntx_.Entry(PromoAction).State = EntityState.Modified;
                cntx_.SaveChanges(CurrentUser, "Удаление промо-акции");

                cntx_.promotionsimage.Remove(cntx_.promotionsimage.Where(xxx => xxx.Id == IdFile).First());
                cntx_.promotions.Remove(PromoAction);
                cntx_.SaveChanges(CurrentUser, "Удаление промо-акции");
            }

            SuccessMessage("Промо-Акция удалена");
            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public ActionResult Delete(long? Id)
        //{
        //    PromotionValidation ViewPromotion = new PromotionValidation();
        //    var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
        //    ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
        //    ProducerInterfaceCommon.Heap.NamesHelper h;
        //    cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
        //    h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);

        //    ViewData["DrugList"] = h.GetCatalogList();

        //    ViewPromotion = cntx_.promotions.Where(xxx => xxx.Id == Id).ToList().Select(xxx => new PromotionValidation { Id = xxx.Id, Name = xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status }).FirstOrDefault();

        //    //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
        //    ViewPromotion.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == Id).ToList().Select(xxx => xxx.DrugId).ToList();

        //    return View(ViewPromotion);
        //}

        //[HttpPost]
        //public ActionResult Delete(PromotionValidation PromoAction)
        //{
        //    cntx_.promotionToDrug.RemoveRange(cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == PromoAction.Id));
        //    cntx_.promotions.Remove(cntx_.promotions.Where(xxx => xxx.Id == PromoAction.Id).First());
        //    cntx_.SaveChanges(CurrentUser);
        //    SuccessMessage("Акция " + PromoAction.Name + " успешно удалена.");
        //    return RedirectToAction("Index");
        //}

        //public FileContentResult GetImage(int Id)
        //{
        //    var Image = cntx_.promotionsimage.Find(Id);
        //    return new FileContentResult(Image.ImageFile, Image.ImageType);
        //}

        //public ActionResult GetViewImage(int Id)
        //{
        //    ViewBag.Id_File = Id;
        //    return PartialView();
        //}

        //[HttpGet]
        //public ActionResult ChangeFile(long Id)
        //{
        //    return PartialView("Partial/FileUploadChange",Id);
        //}

        //[HttpPost]
        //public ActionResult ChangeFile(long Id, HttpPostedFileBase file)
        //{

        //    var PromoFile = new promotionsimage();

        //    if (file == null)
        //    {
        //        ErrorMessage("Файл не был прикреплен к промо-акции");
        //        return RedirectToAction("Index", new { Id = Id.ToString()});
        //    }

        //    MemoryStream MS = new MemoryStream();
        //    file.InputStream.CopyTo(MS);


        //    PromoFile.ImageFile = MS.ToArray();
        //    PromoFile.ImageName = file.FileName.Split(new Char[] { '\\' }).Last();
        //    PromoFile.ImageType = file.ContentType;
        //    PromoFile.ImageSize = MS.Length.ToString();

        //    cntx_.promotionsimage.Add(PromoFile);
        //    cntx_.SaveChanges();

        //    var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == Id).First();

        //    if (PromoAction.PromoFileId == null)
        //    {
        //        PromoAction.promotionsimage = PromoFile;
        //        PromoAction.PromoFile = PromoFile.ImageName;
        //        cntx_.Entry(PromoAction).State = EntityState.Modified;              
        //        cntx_.SaveChanges();
        //    }
        //    else
        //    {
        //        int ImageOldId = (int)PromoAction.PromoFileId;
        //        PromoAction.promotionsimage = PromoFile;
        //        PromoAction.PromoFile = PromoFile.ImageName;
        //        cntx_.Entry(PromoAction).State = EntityState.Modified;
        //        cntx_.SaveChanges();

        //        cntx_.promotionsimage.Remove(cntx_.promotionsimage.Where(xxx => xxx.Id == ImageOldId).First());
        //        cntx_.SaveChanges();
        //    }            

        //    SuccessMessage("Файл добавлен / заменён");
        //    return RedirectToAction("Index", new {Id = Id.ToString()} );
        //}

        //[HttpGet]
        //public ActionResult AddFile(long? Id)
        //{
        //    var promo = cntx_.promotions.Where(xxx => xxx.Id == Id && xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).FirstOrDefault();

        //    if (promo == null || String.IsNullOrEmpty(promo.Name))
        //    {
        //        ErrorMessage("У вас нет прав для редактирования данной промо-акции");
        //        return RedirectToAction("Index");
        //    }
        //    var ImageId = promo.PromoFileId;

        //    string Image = "";

        //    var ViewModel = new PromotionFile();

        //    if (ImageId != null)
        //    {
        //        Image = promo.promotionsimage.ImageName;
        //        ViewModel = new PromotionFile() { Id = promo.Id, ImageFile = Image, FileId = (int)ImageId };
        //    }
        //    else
        //    {
        //        ViewModel = new PromotionFile() { Id = promo.Id, ImageFile = Image };
        //    }
        //    return View(ViewModel);
        //}

        //[HttpPost]
        //public ActionResult AddFile(PromotionFile ChangeOrAddImage, HttpPostedFileBase image)
        //{
        //    var PromoFile = new promotionsimage();

        //    if (image == null)
        //    {
        //        ErrorMessage("Файл не был прикреплен к промо-акции");
        //        return RedirectToAction("Index");
        //    }

        //    MemoryStream MS = new MemoryStream();
        //    image.InputStream.CopyTo(MS);


        //    PromoFile.ImageFile = MS.ToArray();
        //    PromoFile.ImageName = image.FileName.Split(new Char[] { '\\' }).Last();
        //    PromoFile.ImageType = image.ContentType;
        //    PromoFile.ImageSize = MS.Length.ToString();

        //    cntx_.promotionsimage.Add(PromoFile);
        //    cntx_.SaveChanges();

        //    var PromoAction = cntx_.promotions.Where(xxx => xxx.Id == ChangeOrAddImage.Id).First();

        //    if (PromoAction.PromoFileId == null)
        //    {
        //        PromoAction.promotionsimage = PromoFile;
        //        cntx_.Entry(PromoAction).State = EntityState.Modified;
        //        cntx_.SaveChanges();
        //    }
        //    else
        //    {
        //        int ImageOldId = (int)PromoAction.PromoFileId;
        //        PromoAction.promotionsimage = PromoFile;
        //        cntx_.Entry(PromoAction).State = EntityState.Modified;
        //        cntx_.SaveChanges();

        //        cntx_.promotionsimage.Remove(cntx_.promotionsimage.Where(xxx => xxx.Id == ImageOldId).First());
        //        cntx_.SaveChanges();
        //    }

        //    SuccessMessage("");
        //    return RedirectToAction("Index");
        //}

        //public ActionResult PromotionStatus(int Id)
        //{
        //    var Promotion = cntx_.promotions.Find(Id);

        //    if (Promotion.Status == false)
        //    {
        //        return Content("Не подтверждена</p>");
        //    }
        //    else
        //    {
        //        if (Promotion.Begin > DateTime.Now)
        //        {
        //            return Content("Ожидание даты начала акции</p>");
        //        }
        //        else
        //        {
        //            if (Promotion.End < DateTime.Now)
        //            {
        //                return Content("Акция завершена</p>");
        //            }
        //            else
        //            {
        //                if (Promotion.Enabled == true)
        //                {
        //                    return Content("Публикуется</p>");
        //                }
        //                else
        //                {
        //                    return Content("Подтверждена, не публикуется</p>");
        //                }
        //            }
        //        }
        //    }         
        //}


    }
}