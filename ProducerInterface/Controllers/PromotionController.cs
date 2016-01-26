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

namespace ProducerInterface.Controllers
{
    public class PromotionController : pruducercontroller.BaseController
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
        public ActionResult Index()
        {
            var currentUser = GetCurrentUser();
            //var list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId && xxx.Status).ToList();
            IEnumerable<promotions> list = cntx_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId).ToList();
            return View(list);
        }

        [HttpGet]
        public ActionResult Manage(long? id)
        {
            PromotionValidation ViewPromotion = new PromotionValidation();
            var currentUser = GetCurrentUser();

            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
            ProducerInterfaceCommon.Heap.NamesHelper h;
            cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
            h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);

            ViewData["DrugList"] = h.GetCatalogList();
            if (id.HasValue)
            {
                //ViewBag.CurrentPromotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
                // редактирование существующей
                ViewPromotion = cntx_.promotions.Where(xxx => xxx.Id == id).ToList().Select(xxx => new PromotionValidation { Id = xxx.Id, Name = xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status }).FirstOrDefault();

                //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
                ViewPromotion.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == id).ToList().Select(xxx => xxx.DrugId).ToList();
            }
            else {
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

            if (validationChanges)
            {
                SuccessMessage("Акция не изменена");
                return RedirectToAction("Index");
            }
                        
            PromotionSave.Name = PromoAction.Name;
            PromotionSave.Status = false;
            PromotionSave.Annotation = PromoAction.Annotation;
            PromotionSave.Begin = PromoAction.Begin;
            PromotionSave.End = PromoAction.End;
            PromotionSave.ProducerId = (long)CurrentUser.ProducerId;
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
                PromotionSave.ProducerUserId = CurrentUser.Id; // в новую акцию добавляем Id пользователя
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
                SuccessMessage("Акция изменена, в списке отобразится после подтверждения");
                ProducerInterfaceCommon.Heap.EmailSender.SendChangePromotion(cntx_, CurrentUser.Id, PromoAction.Id, CurrentUser.IP);
            }
            else
            {
                // новая акция добавлена
                SuccessMessage("Акция добавлена, в списке отобразится после подтверждения");
                ProducerInterfaceCommon.Heap.EmailSender.SendNewPromotion(cntx_, PromotionSave.ProducerUserId, PromotionSave.Id, CurrentUser.IP);
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(long? Id)
        {
            PromotionValidation ViewPromotion = new PromotionValidation();
            var currentUser = GetCurrentUser();
            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
            ProducerInterfaceCommon.Heap.NamesHelper h;
            cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
            h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, CurrentUser.Id);

            ViewData["DrugList"] = h.GetCatalogList();

            ViewPromotion = cntx_.promotions.Where(xxx => xxx.Id == Id).ToList().Select(xxx => new PromotionValidation { Id = xxx.Id, Name = xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status }).FirstOrDefault();

            //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
            ViewPromotion.DrugList = cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == Id).ToList().Select(xxx => xxx.DrugId).ToList();

            return View(ViewPromotion);
        }

        [HttpPost]
        public ActionResult Delete(PromotionValidation PromoAction)
        {
            cntx_.promotionToDrug.RemoveRange(cntx_.promotionToDrug.Where(xxx => xxx.PromotionId == PromoAction.Id));
            cntx_.promotions.Remove(cntx_.promotions.Where(xxx=>xxx.Id == PromoAction.Id).First());
            cntx_.SaveChanges(CurrentUser);
            SuccessMessage("Акция " + PromoAction.Name + " успешно удалена.");
            return RedirectToAction("Index");
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

            return (Name && Annotation && DataBegin && DataEnd && !ListDrugsInPromotion);
        }


    }
}