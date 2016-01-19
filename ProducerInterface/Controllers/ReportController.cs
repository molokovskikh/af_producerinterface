using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

using Quartz;
using Quartz.Impl;
using Quartz.Job;
using Quartz.Job.Models;

using System.Configuration;
using Common.Logging;
using Quartz.Job.EDM;

using System.Data;
using System.IO;

namespace ProducerInterface.Controllers
{
    public class ReportController : pruducercontroller.BaseController
    {
        //
        // GET: /Report/

        protected static readonly ILog logger = LogManager.GetLogger(typeof(ReportController));

        protected reportData cntx;
        protected NamesHelper h;
        protected long userId;
        protected long producerId;
      
        protected Quartz.IScheduler scheduler;
             
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            cntx = new reportData();
            //  TODO: берётся у юзера            
            try
            {
                userId = CurrentUser.Id;    
                producerId = (long)CurrentUser.ProducerId;
            }
            catch
            {
                // Ignore
            }

            

            h = new NamesHelper(cntx, userId);
            #if DEBUG
            scheduler = GetDebagSheduler();
            #else
			scheduler = GetRemoteSheduler();
            #endif
        }
        


        public ActionResult AddJob(int Id)
        {
            // получили пустую модель нужного типа
            var type = GetModelType(Id);
            var model = (Report)Activator.CreateInstance(type);
            model.Id = Id;
            model.ProducerId = producerId;
            model.MailSubject = h.GetMailOkReportSubject();

            // при GET - отправили её пользователю на заполнение
            if (HttpContext.Request.HttpMethod == "GET")
            {
                SetViewData(model);
                return View(model);
            }

            // при POST - биндим
            Bind(model, type);

            foreach (var error in model.Validate())
                ModelState.AddModelError(error.PropertyName, error.Message);

            // если модель невалидна - возвращаем пользователю
            if (!ModelState.IsValid)
            {
                SetViewData(model);
                return View(model);
            }

            var key = new JobKey(Guid.NewGuid().ToString(), $"p{producerId}");
            var job = JobBuilder.Create<ReportJob>()
                .WithIdentity(key)
                .StoreDurably()
                .WithDescription(model.ToString())
                .Build();
            job.JobDataMap["param"] = model;
            try
            {
                scheduler.AddJob(job, true);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} add failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }

            var userName = cntx.usernames.Single(x => x.UserId == userId).UserName;
            // иначе - успех
            var jext = new jobextend()
            {
                CreationDate = DateTime.Now,
                Creator = userName,
                CustomName = model.CastomName,
                Enable = true,
                JobGroup = key.Group,
                JobName = key.Name,
                LastModified = DateTime.Now,
                ProducerId = producerId,
                ReportType = model.Id,
                SchedName = scheduler.SchedulerName,
                Scheduler = "Расписание не задано"
            };

            cntx.jobextend.Add(jext);
            cntx.SaveChanges();
            return View("Success", (object)"Отчёт успешно добавлен");
        }

        public ActionResult Delete(string jobName, string jobGroup)
        {
            var key = new JobKey(jobName, jobGroup);
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var jext = GetJobExtend(jobName, jobGroup);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            try
            {
                scheduler.PauseJob(key);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} paused failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }
            jext.Enable = false;
            cntx.SaveChanges();
            return View("Success", (object)"Задача удалена");
        }

        public ActionResult Restore(string jobName, string jobGroup)
        {
            var key = new JobKey(jobName, jobGroup);
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var jext = GetJobExtend(jobName, jobGroup);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            try
            {
                scheduler.ResumeJob(key);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} resume failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }
            jext.Enable = true;
            cntx.SaveChanges();
            return View("Success", (object)"Задача восстановлена");
        }

        public ActionResult Edit(string jobName, string jobGroup)
        {
            var key = new JobKey(jobName, jobGroup);
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var jext = GetJobExtend(jobName, jobGroup);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            var model = (Report)job.JobDataMap["param"];

            // при GET - отправили её пользователю на заполнение
            if (HttpContext.Request.HttpMethod == "GET")
            {
                SetViewData(model);
                return View("AddJob", model);
            }

            // при POST - биндим
            var type = GetModelType(jext.ReportType);
            model = (Report)Activator.CreateInstance(type);

            Bind(model, type);
            model.ProducerId = producerId;

            foreach (var error in model.Validate())
                ModelState.AddModelError(error.PropertyName, error.Message);

            // если модель невалидна - возвращаем пользователю
            if (!ModelState.IsValid)
            {
                SetViewData(model);
                return View("AddJob", model);
            }

            // строим новый джоб
            var newJob = JobBuilder.Create<ReportJob>()
                .WithIdentity(key)
                .StoreDurably()
                .WithDescription(model.ToString())
                .Build();
            newJob.JobDataMap["param"] = model;
            try
            {
                // новый джоб заменяет старый
                scheduler.AddJob(newJob, true);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} change failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }

            // вносим изменения в расширенные параметры
            jext.LastModified = DateTime.Now;
            jext.CustomName = model.CastomName;
            cntx.SaveChanges();
            return View("Success", (object)"Отчёт успешно изменён");
        }

        public ActionResult JobList()
        {
            var jobList = cntx.jobextend.Where(x => x.ProducerId == producerId
                                                                                            && x.SchedName == scheduler.SchedulerName
                                                                                            && x.Enable == true).ToList();


            return View(jobList);
        }

        public ActionResult RunNow(string jobName, string jobGroup)
        {
            var key = JobKey.Create(jobName, jobGroup);
            // нашли задачу
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var jext = GetJobExtend(jobName, jobGroup);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            var param = (Report)job.JobDataMap["param"];
            ViewBag.Title = $"Запуск \"{param.CastomName}\"";

            RunNowParam model;
            if (param is IInterval)
                model = new RunNowIntervalParam();
            // TODO при появлении неинтервальных отчётов добавить код
            else
                return View("Error", (object)"Отчёт этого типа невозможно запустить вручную");

            // при GET возвращаем пустую модель для заполнения
            if (HttpContext.Request.HttpMethod == "GET")
                return View(model);

            // биндим
            Bind(model, model.GetType());

            model.UserId = userId;

            foreach (var error in model.Validate())
                ModelState.AddModelError(error.PropertyName, error.Message);

            // если модель невалидна - возвращаем пользователю
            if (!ModelState.IsValid)
                return View(model);

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .StartNow()
                .ForJob(job.Key)
                .Build();
            trigger.JobDataMap["tparam"] = model;

            try
            {
                scheduler.ScheduleJob(trigger);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} run now failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }

            // отправили статус, что отчёт готовится
            jext.DisplayStatusEnum = DisplayStatus.Processed;
            jext.LastRun = DateTime.Now;
            cntx.SaveChanges();

            var message = "АналитФармация приступила к формированию запрошенного отчета. Как только отчет будет готов, он сразу же ";
            if (model.ByDisplay)
                message = message + "появится в списке отчётов";
            if (model.ByDisplay && model.ByEmail)
                message = message + " и ";
            if (model.ByEmail)
                message = message + "будет отправлен на указанные адреса электронной почты";

            return View("Success", (object)message);
        }

        public ActionResult ScheduleJob(string jobName, string jobGroup)
        {
            var key = JobKey.Create(jobName, jobGroup);
            // нашли задачу
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var jext = GetJobExtend(key.Name, key.Group);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            var param = (Report)job.JobDataMap["param"];
            ViewBag.Title = $"Расписание для \"{param.CastomName}\"";

            CronParam model;
            if (param is IInterval)
                model = new CronIntervalParam();
            // TODO при появлении неинтервальных отчётов добавить код
            else
                return View("Error", (object)"Отчёт этого типа невозможно запустить вручную");

            // триггер вставлялся с идентификатором задачи
            var oldTriggerKey = new TriggerKey(key.Name, key.Group);
            // используются только cron-триггеры
            var oldTrigger = (ICronTrigger)(scheduler.GetTrigger(oldTriggerKey));

            // при GET возвращаем пустую модель для заполнения
            if (HttpContext.Request.HttpMethod == "GET")
            {
                //// если триггер уже был - устанавливаем UI его значением
                if (oldTrigger != null)
                    model = (CronParam)oldTrigger.JobDataMap["tparam"];
                //if (oldTrigger != null)
                //	model.CronExpression = oldTrigger.CronExpressionString;
                return View(model);
            }

            // биндим
            Bind(model, model.GetType());

            model.UserId = userId;

            foreach (var error in model.Validate())
                ModelState.AddModelError(error.PropertyName, error.Message);

            // если модель невалидна - возвращаем пользователю
            if (!ModelState.IsValid)
                return View(model);

            // новый триггер для этой задачи
            var trigger = TriggerBuilder.Create()
                .WithIdentity(key.Name, key.Group)
                .WithCronSchedule(model.CronExpression)
                .ForJob(job.Key)
                .WithDescription(model.CronHumanText)
                .Build();
            trigger.JobDataMap["tparam"] = model;

            try
            {
                if (oldTrigger == null)
                    scheduler.ScheduleJob(trigger);
                else
                    scheduler.RescheduleJob(oldTriggerKey, trigger);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} run now failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }
            // меняем человекочитаемое описание в доп. параметрах задачи
            jext.Scheduler = model.CronHumanText;
            cntx.SaveChanges();
            return View("Success", (object)"Расписание успешно установлено");
        }

        public ActionResult DisplayReport(string jobName, string jobGroup)
        {
            var jxml = cntx.reportxml.SingleOrDefault(x => x.JobName == jobName && x.JobGroup == jobGroup);
            if (jxml == null)
                return View("Error", (object)"Отчёт не найден");

            var ds = new DataSet();
            ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
            return View(ds);
        }

        public JsonResult GetSupplierJson(List<decimal> RegionCodeEqual, List<long> SupplierIdNonEqual)
        {
            if (SupplierIdNonEqual == null)
                SupplierIdNonEqual = new List<long>();

            var supplierStringList = SupplierIdNonEqual.Select(x => x.ToString()).ToList();
            return Json(new
            {
                results = (h.GetSupplierList(RegionCodeEqual).Select(x => new { value = x.Value, text = x.Text, selected = supplierStringList.Contains(x.Value) }))
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDisplayStatusJson(string jobName, string jobGroup)
        {
            var jext = GetJobExtend(jobName, jobGroup);
            return Json(new
            {
                status = jext.DisplayStatus,
                url = Url.Action("DisplayReport", "Report", new { jobName = jobName, jobGroup = jobGroup })
            }, JsonRequestBehavior.AllowGet);
        }

        // доп. инфа по джобу
        protected jobextend GetJobExtend(string jobName, string jobGroup)
        {
            return cntx.jobextend.SingleOrDefault(x => x.SchedName == scheduler.SchedulerName
                                                                                                && x.JobName == jobName
                                                                                                && x.JobGroup == jobGroup
                                                                                                && x.ProducerId == producerId
                                                                                                && x.Enable == true);
        }

        protected Type GetModelType(int id)
        {
            var typeName = ((Reports)id).ToString();
            var type = Type.GetType($"Quartz.Job.Models.{typeName}, {typeof(Report).Assembly.FullName}");
            if (type == null)
                throw new NotSupportedException($"Не удалось создать тип {typeName} по идентификатору {id}");
            return type;
        }

        protected void Bind(object model, Type type)
        {
            this.Binders.GetBinder(type).BindModel(this.ControllerContext, new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType((Func<object>)(() => model), type),
                ModelName = (string)null,
                ModelState = this.ModelState,
                PropertyFilter = (Predicate<string>)(propertyName => true),
                ValueProvider = this.ValueProvider
            });
        }

        protected void SetViewData(Report model)
        {
            foreach (var item in model.ViewDataValues(h))
                ViewData[item.Key] = item.Value;
        }

        //	logger.Debug($"Start setting job {model.JobGroup} {model.JobName}");

        protected IScheduler GetDebagSheduler()
        {
            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzDebug");
            var sf = new StdSchedulerFactory(props);
            var scheduler = sf.GetScheduler();

            // проверяем имя шедулера
            if (scheduler.SchedulerName != "TestScheduler")
                throw new NotSupportedException("Должен использоваться TestScheduler");

            // проверяем локальный ли шедулер
            var metaData = scheduler.GetMetaData();
            if (metaData.SchedulerRemote)
                throw new NotSupportedException("Должен использоваться локальный TestScheduler");

            if (!scheduler.IsStarted)
                scheduler.Start();
            return scheduler;
        }

        protected IScheduler GetRemoteSheduler()
        {
            var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
            var sf = new StdSchedulerFactory(props);
            var scheduler = sf.GetScheduler();

            // проверяем имя шедулера
            if (scheduler.SchedulerName != "ServerScheduler")
                throw new NotSupportedException("Должен использоваться ServerScheduler");

            // проверяем удалённый ли шедулер
            var metaData = scheduler.GetMetaData();
            if (!metaData.SchedulerRemote)
                throw new NotSupportedException("Должен использоваться удалённый ServerScheduler");

            return scheduler;
        }
    }
}
