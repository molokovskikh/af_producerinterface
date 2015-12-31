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

namespace ProducerInterface.Controllers
{
    public class ProfileController : pruducercontroller.BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.BreadCrumb = "Профиль пользователя";            
        }
    

        public ActionResult Index()
        {
            //var ModelView = new Models.RegistrerValidation();
            //Quartz.Job.EDM.reportData cntx;
            //Quartz.Job.NamesHelper h;
            //cntx = new Quartz.Job.EDM.reportData();
            //h = new Quartz.Job.NamesHelper(cntx, 0);
            //scheduler = GetRemoteSheduler();

            //var jobList = cntx.jobextend.Where(x => x.ProducerId == CurrentUser.ProducerId
            //                                                                            && x.SchedName == scheduler.SchedulerName
            //                                                                            && x.Enable == true).ToList();

            //return View(jobList);
            return View();
        }

        public ActionResult Promotions()
        {
            var currentUser = GetCurrentUser();
            //var list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId && xxx.Status).ToList();
            IEnumerable<Models.promotions> list = _BD_.promotions.Where(xxx => xxx.ProducerId == currentUser.ProducerId).ToList();        
            return View(list);
        }

        public ActionResult ManagePromotion(long? id)
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
                 ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, Status = xxx.Status}).FirstOrDefault();
        
                //ViewPromotion = _BD_.promotions.Where(xxx=>xxx.Id == id).ToList().Select(xxx=> new PromotionValidation {Id=xxx.Id, Name= xxx.Name, Annotation = xxx.Annotation, Begin = xxx.Begin, End = xxx.End, DrugList = xxx.DrugList, Status = xxx.Status}).FirstOrDefault();
                ViewPromotion.DrugList = _BD_.promotiontodrug.Where(xxx => xxx.PromotionId == id).ToList().Select(xxx => xxx.DrugId).ToList();
            }
            else {              
                // Создание новой акции нужное значение уже присвоено
                //var CurrentPromotion = new promotions();
            }
            int TEMPO = 1;
            return View(ViewPromotion);
        }

        [HttpPost]
        public ActionResult ManagePromotion(PromotionValidation New_and_Edit_Promotion)
        {
            var model = New_and_Edit_Promotion;
                   

            if (ModelState.IsValid)
            {
                produceruser user_ = GetCurrentUser();
                promotions NewPromotion = new promotions();

                NewPromotion.UpdateTime = SystemTime.Now();
                NewPromotion.ProducerId = (long)user_.ProducerId;
                NewPromotion.ProducerUserId = user_.Id;
                NewPromotion.Status = false;

                NewPromotion.Name = model.Name;
                NewPromotion.Annotation = model.Annotation;
                NewPromotion.Begin = model.Begin;
                NewPromotion.End = model.End;
                NewPromotion.RegionMask = 1;
           //     NewPromotion.DrugList = model.DrugList;            

                if (model.Id == 0)
                {
                    model.Id = default(long);
                    NewPromotion.Id = default(long);
                    _BD_.Entry(NewPromotion).State = System.Data.Entity.EntityState.Added;
                    _BD_.SaveChanges();                                
                }
                else
                { 
                    NewPromotion.Id = model.Id;
                    _BD_.Entry(NewPromotion).State = System.Data.Entity.EntityState.Modified;
                    List<promotiontodrug> LST = _BD_.promotiontodrug.Where(xxx => xxx.PromotionId == model.Id).ToList();

                    foreach (var DrugInPromotion in LST)
                    {
                        _BD_.promotiontodrug.Remove(DrugInPromotion);
                    }                 
                }

                foreach (var X in model.DrugList)
                {
                    _BD_.Entry(new Models.promotiontodrug { DrugId = X, PromotionId = model.Id }).State = System.Data.Entity.EntityState.Added;
                }
                _BD_.SaveChanges();

                SuccessMessage("Акция добавлена(изменена), в списке отобразится после подсверждения");
                return RedirectToAction("Promotions", "Profile");

            }
            else
            {
                Quartz.Job.EDM.reportData cntx;
                Quartz.Job.NamesHelper h;
                cntx = new Quartz.Job.EDM.reportData();
                h = new Quartz.Job.NamesHelper(cntx, CurrentUser.Id);

                ViewData["DrugList"] = h.GetCatalogList();
                return View(model);
            }
        }







    }
}