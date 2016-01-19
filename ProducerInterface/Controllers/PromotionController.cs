using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using Quartz.Job.EDM;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using Quartz.Job.Models;
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
            IEnumerable<Models.promotions> list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId).ToList();
            return View(list);
        }

        [HttpGet]
        public ActionResult Manage(long? id)
        {
            PromotionValidation ViewPromotion = new PromotionValidation();
            var currentUser = GetCurrentUser();

            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            Quartz.Job.EDM.reportData cntx;
            Quartz.Job.NamesHelper h;
            cntx = new Quartz.Job.EDM.reportData();
            h = new Quartz.Job.NamesHelper(cntx, CurrentUser.Id);

            ViewData["DrugList"] = h.GetCatalogList();
            if (id.HasValue)
            {
                //ViewBag.CurrentPromotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
                // редактирование существующей
                ViewPromotion = _BD_.promotions.Where(xxx => xxx.Id == id).ToList().Select(xxx => new PromotionValidation { Id = xxx.Id, Name = xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status }).FirstOrDefault();

                //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
                ViewPromotion.DrugList = _BD_.promotionToDrug.Where(xxx => xxx.PromotionId == id).ToList().Select(xxx => xxx.DrugId).ToList();
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

            var ListGRUGSSS = _BD_.promotionToDrug.Where(xxx => xxx.PromotionId == 13).ToList();

            if (ModelState.IsValid)
            {
                produceruser user_ = GetCurrentUser();
                promotions NewPromotion = new promotions();

                NewPromotion.UpdateTime = SystemTime.Now();
                NewPromotion.ProducerId = (long)user_.ProducerId;
                NewPromotion.ProducerUserId = user_.Id;
                NewPromotion.Status = false;

                NewPromotion.Name = PromoAction.Name;
                NewPromotion.Annotation = PromoAction.Annotation;
                NewPromotion.Begin = PromoAction.Begin;
                NewPromotion.End = PromoAction.End;
                NewPromotion.RegionMask = 1;

                if (PromoAction.Id == 0)
                {
                    PromoAction.Id = default(long);
                    NewPromotion.Id = default(long);
                    _BD_.Entry(NewPromotion).State = System.Data.Entity.EntityState.Added;
                    _BD_.SaveChanges();

                    SuccessMessage("Акция добавлена, в списке отобразится после подтверждения");
                }
                else
                {
                    NewPromotion.Id = PromoAction.Id;
                    _BD_.Entry(NewPromotion).State = EntityState.Modified;
                    long XXX = PromoAction.Id;                  
                    _BD_.promotionToDrug.RemoveRange(_BD_.promotionToDrug.Where(xxx => xxx.PromotionId == XXX).ToList());               
                    _BD_.SaveChanges();
                    SuccessMessage("Акция изменена, в списке отобразится после подтверждения");
                }

                var ID_Promotion = NewPromotion.Id;
                foreach (var X in PromoAction.DrugList)
                {
                    var DrugInPromotion = new promotionToDrug() { DrugId = X, PromotionId = ID_Promotion };                    
                    _BD_.promotionToDrug.Add(DrugInPromotion);      
                    
                    // привязка лекарств к акции           
                }

                _BD_.SaveChanges();
             
                return RedirectToAction("Index");
            }
            else
            {
                Quartz.Job.EDM.reportData cntx;
                Quartz.Job.NamesHelper h;
                cntx = new Quartz.Job.EDM.reportData();
                h = new Quartz.Job.NamesHelper(cntx, CurrentUser.Id);
                ViewData["DrugList"] = h.GetCatalogList();
                return View(PromoAction);
            }
         
        }

        [HttpGet]
        public ActionResult Delete(long? Id)
        {
            PromotionValidation ViewPromotion = new PromotionValidation();
            var currentUser = GetCurrentUser();
            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            Quartz.Job.EDM.reportData cntx;
            Quartz.Job.NamesHelper h;
            cntx = new Quartz.Job.EDM.reportData();
            h = new Quartz.Job.NamesHelper(cntx, CurrentUser.Id);

            ViewData["DrugList"] = h.GetCatalogList();

            ViewPromotion = _BD_.promotions.Where(xxx => xxx.Id == Id).ToList().Select(xxx => new PromotionValidation { Id = xxx.Id, Name = xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status }).FirstOrDefault();

            //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
            ViewPromotion.DrugList = _BD_.promotionToDrug.Where(xxx => xxx.PromotionId == Id).ToList().Select(xxx => xxx.DrugId).ToList();

            return View(ViewPromotion);
        }

        [HttpPost]
        public ActionResult Delete(PromotionValidation PromoAction)
        {
            _BD_.promotionToDrug.RemoveRange(_BD_.promotionToDrug.Where(xxx => xxx.PromotionId == PromoAction.Id));
            _BD_.promotions.Remove(_BD_.promotions.Where(xxx=>xxx.Id == PromoAction.Id).First());
            _BD_.SaveChanges();
            SuccessMessage("Акция " + PromoAction.Name + " успешно удалена.");
            return RedirectToAction("Index");
        }


    }
}