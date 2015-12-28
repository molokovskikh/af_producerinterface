using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

using Quartz;

using Quartz.Impl;
using Quartz.Job;
using Quartz.Job.Models;

using System.Configuration;
using Common.Logging;
using Quartz.Job.EDM;


namespace ProducerInterface.Controllers
{
    public class ReportController : BaseProducerInterfaceController
    {
        //
        // GET: /Report/

        protected static readonly ILog logger = LogManager.GetLogger(typeof(ReportController));

        protected reportData cntx;
        protected long userId;
        protected long producerId;
        protected NamesHelper h;
        protected Quartz.IScheduler scheduler;
             
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            cntx = new reportData();
          //  TODO: берётся у юзера
            var CurrentUser = GetCurrentUser();
            userId = 1; // CurrentUser.Id;
            producerId = 55;  // CurrentUser.Producer.Id;
          //  cntx.usernames.Single(x => x.UserId == userId).ProducerId;

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
            var model = (Quartz.Job.Models.Report)Activator.CreateInstance(type);
            model.Id = Id;
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

            var key = new JobKey(Guid.NewGuid().ToString(), userId.ToString());
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

            var model = (Quartz.Job.Models.Report)job.JobDataMap["param"];

            // при GET - отправили её пользователю на заполнение
            if (HttpContext.Request.HttpMethod == "GET")
            {
                SetViewData(model);
                return View("AddJob", model);
            }

            // при POST - биндим
            var type = GetModelType(jext.ReportType);
            model = (Quartz.Job.Models.Report)Activator.CreateInstance(type);

            Bind(model, type);

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
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            var param = (Quartz.Job.Models.Report)job.JobDataMap["param"];

            if (param is IntervalReport)
            {
                var model = new SetDateInterval()
                {
                    JobName = key.Name,
                    JobGroup = key.Group
                };
                var castparam = (IntervalReport)param;
                var now = DateTime.Now;
                // за предыдущий месяц
                if (castparam.ByPreviousMonth)
                {
                    // по : 00:00:00 первый день текущего месяца
                    model.DateTo = new DateTime(now.Year, now.Month, 1);
                    // с: 00:00:00 первый день предыдущего месяца
                    model.DateFrom = model.DateTo.AddMonths(-1);
                }
                // за X предыдущих дней от момента запуска
                else {
                    // по: 00:00:00 сегодня
                    model.DateTo = new DateTime(now.Year, now.Month, now.Day);
                    // с: 00:00:00 за Interval дней
                    model.DateFrom = model.DateTo.AddDays(-castparam.Interval.Value);
                }
                return View(model);
            }
            // TODO при появлении неинтервальных отчётов добавить код
            else
                return View("Error", (object)"Отчёт этого типа невозможно запустить вручную");
        }

        [HttpPost]
        public ActionResult RunNow(SetDateInterval model)
        {
            if (model.DateToUi < model.DateFrom)
                ModelState.AddModelError("DateToUi", "Дата \"с...\" должна быть меньше или равна дате \"по...\"");
            if (!ModelState.IsValid)
                return View(model);

            var key = JobKey.Create(model.JobName, model.JobGroup);
            var job = scheduler.GetJobDetail(key);
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            // строим новый джоб
            var newJob = JobBuilder.Create<ReportJob>()
                .WithIdentity(key)
                .StoreDurably()
                .WithDescription(model.ToString())
                .Build();

            var param = (Quartz.Job.Models.Report)job.JobDataMap["param"];
            if (param is IntervalReport)
            {
                var castparam = (IntervalReport)param;
                castparam.DateFrom = model.DateFrom;
                castparam.DateTo = model.DateTo;
                newJob.JobDataMap["param"] = castparam;
            }
            // TODO при появлении неинтервальных отчётов добавить код
            else
                return View("Error", (object)"Отчёт этого типа невозможно запустить вручную");

            try
            {
                // новый джоб заменяет старый
                scheduler.AddJob(newJob, true);
                // новый джоб заменяет старый
                scheduler.TriggerJob(key);
            }
            catch (Exception e)
            {
                logger.Error($"Job {key.Name} {key.Group} run now failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }

            return View("Success", (object)"АналитФармация приступила к формированию запрошенного отчета. Как только отчет будет готов, он сразу же будет отправлен на указанные адреса электронной почты");
        }

        public ActionResult ScheduleJob(string jobName, string jobGroup)
        {
            var model = new Cron();
            model.JobName = jobName;
            model.JobGroup = jobGroup;
            // значение по умолчанию для UI - без первого нуля для секунд и без знаков "?"
            model.CronExpression = "0 10 * * 1";

            // если триггер вставлялся, то с идентификатором задачи
            var oldTriggerKey = new TriggerKey(jobName, jobGroup);
            // используются только cron-триггеры
            var oldTrigger = (ICronTrigger)(scheduler.GetTrigger(oldTriggerKey));
            // если триггер уже был - устанавливаем UI его значением
            if (oldTrigger != null)
                model.CronExpression = FormatCronForUI(oldTrigger.CronExpressionString);

            return View(model);
        }

        [HttpPost]
        public ActionResult ScheduleJob(Cron model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var jext = GetJobExtend(model.JobName, model.JobGroup);
            if (jext == null)
                return View("Error", (object)"Дополнительные параметры задачи не найдены");

            var job = scheduler.GetJobDetail(JobKey.Create(model.JobName, model.JobGroup));
            if (job == null)
                return View("Error", (object)"Задача не найдена");

            // новый триггер для этой задачи
            var trigger = TriggerBuilder.Create()
                .WithIdentity(model.JobName, model.JobGroup)
                .WithCronSchedule(model.CronExpression)
                .ForJob(job.Key)
                .WithDescription(model.CronHumanText)
                .Build();

            // триггер вставлялся с идентификатором задачи
            var oldTriggerKey = new TriggerKey(model.JobName, model.JobGroup);
            var oldTrigger = scheduler.GetTrigger(oldTriggerKey);

            try
            {
                if (oldTrigger == null)
                    scheduler.ScheduleJob(trigger);
                else
                    scheduler.RescheduleJob(oldTriggerKey, trigger);
            }
            catch (Exception e)
            {
                logger.Error($"Job {model.JobGroup} {model.JobName} scheduler add failed:" + e.Message, e);
                return View("Error", (object)(e.Message));
            }
            // меняем человекочитаемое описание в доп. параметрах задачи
            jext.Scheduler = model.CronHumanText;
            cntx.SaveChanges();
            return View("Success", (object)"Расписание успешно установлено");
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
            var type = Type.GetType($"Quartz.Job.Models.{typeName}, {typeof(Quartz.Job.Models.Report).Assembly.FullName}");
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

        protected void SetViewData(Quartz.Job.Models.Report model)
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

        private string FormatCronForUI(string cron)
        {
            return cron.Substring(2).Replace("?", "*");
        }
    }
}
