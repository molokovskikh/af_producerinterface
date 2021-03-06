﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Quartz;
using ProducerInterfaceCommon.Heap;
using Common.Logging;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using System.Data;
using System.IO;
using NHibernate.Linq;
using ProducerInterfaceCommon.Controllers;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterface.Controllers
{
	public class ReportController : BaseController
	{
		private ReportHelper helper;
		protected static readonly ILog logger = LogManager.GetLogger(typeof(ReportController));
		protected NamesHelper h;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			if (CurrentUser == null)
				return;

			helper = new ReportHelper(DB);
			h = new NamesHelper(CurrentUser.Id);
			if (CurrentUser.IsProducer)
				ViewBag.Producernames = DB.producernames.Single(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId)
					.ProducerName;
		}

		public FileResult GetFile(Controller controller, string jobName)
		{
			var jext = DB.jobextend.Single(x => x.JobName == jobName);
			var file = helper.GetExcel(jext);

			// вернули файл
			byte[] fileBytes = System.IO.File.ReadAllBytes(file.FullName);
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			return File(fileBytes, contentType, file.Name);
		}

		/// <summary>
		/// Возвращает форму указанного типа для заполнения параметров отчета, проверяет их, сохраняет задание в шедулере Quartz
		/// </summary>
		/// <param name="Id">Идентификатор типа отчета</param>
		/// <returns></returns>
		public ActionResult AddJob(int? Id)
		{
			if (!Id.HasValue)
			{
				ErrorMessage("Выберите тип отчета");
				return RedirectToAction("Index", "Report");
			}

			// получили пустую модель нужного типа
			var type = helper.GetModelType(Id.Value);
			var model = (Report)Activator.CreateInstance(type);
			model.Id = Id.Value;
			model.Init(CurrentUser);

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

			var scheduler = helper.GetScheduler();

			string group;
			if (CurrentUser.IsProducer)
				group = $"p{CurrentUser.AccountCompany.ProducerId}";
			else
				group = $"u{CurrentUser.Id}";
			var key = new JobKey(Guid.NewGuid().ToString(), group);
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
				logger.Error("Ошибка при добавлении отчета", e);
				ErrorMessage("Непредвиденная ошибка при добавлении отчета");
				return RedirectToAction("Index", "Report");
			}

			// иначе - успех
			var jext = new jobextend(CurrentUser)
			{
				CustomName = model.CastomName,
				JobGroup = key.Group,
				JobName = key.Name,
				ReportType = model.Id,
				SchedName = scheduler.SchedulerName,
				Scheduler = "Время формирования не задано"
			};

			DB.jobextend.Add(jext);
			DB.SaveChanges(CurrentUser, "Добавление отчета");
			SuccessMessage("Отчет успешно добавлен");
			return RedirectToAction("Index", "Report");
		}

		/// <summary>
		/// Ставит задание на паузу в Quartz, в расширенных параметрах задания снимает флаг Enable
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult DeleteOld()
		{
			//выборка
			var schedulerName = ReportHelper.GetSchedulerName();
			IQueryable<Job> query;
			var deleteOldReportsTerm = int.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
			long[] userIds;
			if (CurrentUser.IsProducer) {
				// вытащили всех создателей, создававших отчеты этого производителя
				userIds = DB.jobextend
					.Where(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId && x.SchedName == schedulerName && x.Enable)
					.Select(x => x.CreatorId)
					.Distinct().ToArray();
				query = DbSession.Query<Job>().Fetch(x => x.Owner).Fetch(x => x.Producer)
					.Where(x => x.Producer.Id == CurrentUser.AccountCompany.ProducerId);
			} else {
				userIds = new[] {CurrentUser.Id};
				query = DbSession.Query<Job>().Where(x => x.Owner.Id == CurrentUser.Id);
			}
			ViewData["creators"] = DB.Account.Where(x => userIds.Contains(x.Id)).
				Select(x => new SelectListItem() {Text = x.Name + "(" + x.Login + ")", Value = x.Id.ToString()}).ToList();

			var lastDate = SystemTime.Now().AddMonths(-deleteOldReportsTerm);
			var list = query.Where(x => x.SchedName == schedulerName && x.Enable).ToList();
			list = list.Where(x => x.LastRun.HasValue && x.LastRun.Value < lastDate).
				OrderBy(x => x.LastRun).ToList();
			//удаление
			foreach (var item in list) {
				var jobName = item.JobName;
				var jobGroup = item.JobGroup;
				var jext = GetJobExtend(jobName);
				if (jext == null) {
					logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
					ErrorMessage("Отчет не найден");
					return RedirectToAction("Index", "Report");
				}
				if (jext.CreatorId != CurrentUser.Id) {
					continue;
				}
				var key = new JobKey(jobName, jobGroup);
				var scheduler = helper.GetScheduler();
				var job = scheduler.GetJobDetail(key);
				if (job == null) {
					logger.Error($"Отчет {jobName} не найден");
					ErrorMessage("Отчет не найден");
					return RedirectToAction("Index", "Report");
				}
				try {
					scheduler.PauseJob(key);
				} catch (Exception e) {
					logger.Error($"Ошибка при удалении отчета {jobName}", e);
					ErrorMessage("Непредвиденная ошибка при удалении отчета");
					return RedirectToAction("Index", "Report");
				}
				jext.Enable = false;
				DB.SaveChanges(CurrentUser, "Удаление отчета");
			}
			SuccessMessage("Отчеты, не запускавшиеся больше года, удалены");
			return RedirectToAction("Index", "Report");
		}

		/// <summary>
		/// Ставит задание на паузу в Quartz, в расширенных параметрах задания снимает флаг Enable
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult Delete(string jobName, string jobGroup)
		{
			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			if (jext.CreatorId != CurrentUser.Id && !CurrentUser.IsAdmin)
			{
				ErrorMessage("Вы можете удалять только свои отчеты");
				return RedirectToAction("Index", "Report");
			}

			var key = new JobKey(jobName, jobGroup);
			var scheduler = helper.GetScheduler();
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			try
			{
				scheduler.PauseJob(key);
			}
			catch (Exception e)
			{
				logger.Error($"Ошибка при удалении отчета {jobName}", e);
				ErrorMessage("Непредвиденная ошибка при удалении отчета");
				return RedirectToAction("Index", "Report");
			}
			jext.Enable = false;
			DB.SaveChanges(CurrentUser, "Удаление отчета");
			SuccessMessage("Отчет удален");
			return RedirectToAction("Index", "Report");

		}

		/// <summary>
		/// Производит действия, обратные методу Delete: снимает задание с паузы, устанавливает бит Enable. В настоящий момент в интерфейсе не используется
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult Restore(string jobName, string jobGroup)
		{
			var key = new JobKey(jobName, jobGroup);
			var scheduler = helper.GetScheduler();
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			try
			{
				scheduler.ResumeJob(key);
			}
			catch (Exception e)
			{
				logger.Error($"Ошибка при восстановлении отчета {key.Name}", e);
				ErrorMessage("Непредвиденная ошибка при восстановлении отчета");
				return RedirectToAction("Index", "Report");
			}
			jext.Enable = true;
			DB.SaveChanges(CurrentUser, "Восстановление отчета");
			SuccessMessage("Отчет восстановлен");
			return RedirectToAction("Index", "Report");
		}

		/// <summary>
		/// Возвращает параметры отчета, сохранённые в указанном задании, для редактирования, проверяет их, сохраняет изменения
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult Edit(string jobName, string jobGroup)
		{
			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			if (jext.CreatorId != CurrentUser.Id && !CurrentUser.IsAdmin)
			{
				ErrorMessage("Вы можете редактировать только свои отчеты");
				return RedirectToAction("Index", "Report");
			}

			var scheduler = helper.GetScheduler();
			var key = new JobKey(jobName, jobGroup);
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var model = (Report)job.JobDataMap["param"];

			// при GET - отправили её пользователю на заполнение
			if (HttpContext.Request.HttpMethod == "GET")
			{
				SetViewData(model);
				return View("AddJob", model);
			}

			// при POST - биндим
			var type = helper.GetModelType(jext.ReportType);
			model = (Report)Activator.CreateInstance(type);

			Bind(model, type);
			model.ProducerId = CurrentUser.AccountCompany.ProducerId;

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
				logger.Error($"Ошибка при изменении отчета {key.Name}", e);
				ErrorMessage("Непредвиденная ошибка при изменении отчета");
				return RedirectToAction("Index", "Report");
			}

			// вносим изменения в расширенные параметры
			jext.LastModified = DateTime.Now;
			jext.CustomName = model.CastomName;
			DB.SaveChanges(CurrentUser, "Редактирование отчета");
			SuccessMessage("Отчет успешно изменен");
			return RedirectToAction("Index", "Report");
		}

		[HttpGet]
		public ActionResult Index(long? cid)
		{
			ViewData["descr"] = AvailableReports();
			return View(cid);
		}

		[HttpPost]
		public ActionResult Index(int? id)
		{
			if (id.HasValue)
				return RedirectToAction("AddJob", "Report", new { id = id.Value });

			ModelState.AddModelError("id", "Выберите тип отчета, который хотите создать");
			var reports = AvailableReports();
			ViewData["descr"] = reports;
			return View();
		}

		private List<ReportDescription> AvailableReports()
		{
			var userRegions = CurrentUser.AccountRegion.Select(x => x.RegionId).ToList();
			var reportIds = DB.ReportRegion.ToList().Where(x => userRegions.Contains(x.RegionId)).Select(x => x.ReportId).Distinct().ToList();
			var result = DB.ReportDescription.Where(x => reportIds.Contains(x.Id)).ToList();
			return result;
		}

		/// <summary>
		/// Выводит список заданий производителя
		/// </summary>
		/// <returns></returns>
		public ActionResult SearchResult(long? cid)
		{
			var schedulerName = ReportHelper.GetSchedulerName();
			long[] userIds;
			IQueryable<Job> query;
			if (CurrentUser.IsProducer) {
				// вытащили всех создателей, создававших отчеты этого производителя
				userIds = DB.jobextend
					.Where(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId && x.SchedName == schedulerName && x.Enable)
					.Select(x => x.CreatorId)
					.Distinct().ToArray();
				query = DbSession.Query<Job>().Fetch(x => x.Owner).Fetch(x => x.Producer)
					.Where(x => x.Producer.Id == CurrentUser.AccountCompany.ProducerId);
			} else {
				userIds = new []{ CurrentUser.Id };
				query = DbSession.Query<Job>().Where(x => x.Owner.Id == CurrentUser.Id);
			}
			ViewData["creators"] = DB.Account.Where(x => userIds.Contains(x.Id)).
				Select(x => new SelectListItem() { Text = x.Name + "(" + x.Login + ")", Value = x.Id.ToString() }).ToList();

			if (cid != null)
					query = query.Where(x => x.Owner.Id == cid.Value);
			var items = query.Where(x => x.SchedName == schedulerName && x.Enable)
				.OrderByDescending(x => x.CreationDate).ToList();

				var deleteOldReportsTerm = int.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
			ViewData["DeleteOldReportsTerm"] = deleteOldReportsTerm;
			ViewData["OldReportsExists"] = items.Any(s=> s.Owner.Id == CurrentUser.Id && s.LastRun.HasValue && s.LastRun.Value < SystemTime.Now().AddMonths(-deleteOldReportsTerm));
			return View(items);
		}

		/// <summary>
		/// Возвращает для заполнения форму установки параметров запуска отчета, проверяет их, запускает отчет
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult RunNow(string jobName, string jobGroup)
		{
			var key = JobKey.Create(jobName, jobGroup);
			var scheduler = helper.GetScheduler();
			// нашли задачу
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Запуск \"{param.CastomName}\"";

			RunNowParam model;
			if (param is IInterval)
				model = new RunNowIntervalParam();
			else
				model = new RunNowNotIntervalParam();

			// при GET возвращаем пустую модель для заполнения
			if (HttpContext.Request.HttpMethod == "GET")
			{
				// по умолчанию выделен email пользователя
				model.MailTo = new List<string>() { CurrentUser.Login };
				SetViewData(model);
				return View(model);
			}

			// биндим
			Bind(model, model.GetType());

			model.UserId = CurrentUser.Id;
			model.Ip = CurrentUser.IP;

			foreach (var error in model.Validate())
				ModelState.AddModelError(error.PropertyName, error.Message);

			// если модель невалидна - возвращаем пользователю
			if (!ModelState.IsValid)
			{
				SetViewData(model);
				return View(model);
			}

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
				logger.Error($"Непредвиденная ошибка при запуске отчета {key.Name}", e);
				ErrorMessage("Непредвиденная ошибка при запуске отчета");
				return RedirectToAction("Index", "Report");
			}

			// отправили статус, что отчет готовится
			jext.DisplayStatusEnum = DisplayStatus.Processed;
			jext.LastRun = DateTime.Now;
			DB.SaveChanges();

			var message = "АналитФармация приступила к формированию запрошенного отчета. Как только отчет будет готов, он сразу же появится в списке отчетов";
			if (model.MailTo != null && model.MailTo.Count > 0)
				message = message + " и будет отправлен на указанные email";

			SuccessMessage(message);
			return RedirectToAction("Index", "Report");
		}


		/// <summary>
		/// Возвращает историю запуска отчета
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult RunHistory(string jobName)
		{
			ViewData["repName"] = $"История запусков отчета \"{h.GetReportName(jobName)}\"";
			var model = DB.reportrunlogwithuser.Where(x => x.JobName == jobName).OrderByDescending(x => x.RunStartTime).ToList();
			return View(model);
		}

		/// <summary>
		/// Возвращает форму для заполнения параметров запуска отчетов, устанавливает исполнение задания
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult ScheduleJob(string jobName, string jobGroup)
		{
			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			if (jext.CreatorId != CurrentUser.Id && !CurrentUser.IsAdmin)
			{
				ErrorMessage("Вы можете редактировать только свои отчеты");
				return RedirectToAction("Index", "Report");
			}

			// нашли задачу
			var scheduler = helper.GetScheduler();
			var key = JobKey.Create(jobName, jobGroup);
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Установка времени формирования для отчета \"{param.CastomName}\"";

			CronParam model;
			if (param is IInterval)
				model = new CronIntervalParam();
			else
				model = new CronNotIntervalParam();

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
				SetViewData(model);
				return View(model);
			}

			// биндим
			Bind(model, model.GetType());

			model.UserId = CurrentUser.Id;

			foreach (var error in model.Validate())
				ModelState.AddModelError(error.PropertyName, error.Message);

			// если модель невалидна - возвращаем пользователю
			if (!ModelState.IsValid)
			{
				SetViewData(model);
				return View(model);
			}

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
				logger.Error($"Ошибка при установке расписания отчета {key.Name}", e);
				ErrorMessage("Непредвиденная ошибка при установке расписания");
				return RedirectToAction("Index", "Report");
			}

			var nextFireTimeUtc = trigger.GetNextFireTimeUtc();
			var nextGen = "";
			if (nextFireTimeUtc.HasValue)
				nextGen = $". Время ближайшей автоматической генерации отчета {nextFireTimeUtc.Value.LocalDateTime}";

			// меняем человекочитаемое описание в доп. параметрах задачи
			jext.Scheduler = model.CronHumanText;
			DB.SaveChanges(CurrentUser, "Установка времени формирования отчета");
			SuccessMessage($"Время формирования отчета успешно установлено{nextGen}");
			return RedirectToAction("Index", "Report");
		}

		[HttpGet]
		public ActionResult DisplayReport(string jobName)
		{
			// вытащили отчет
			var jxml = DB.reportxml.Where(x => x.JobName == jobName).Select(x => x.JobName).SingleOrDefault();
			if (jxml == null)
			{
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			ViewBag.Title = jext.CustomName;

			// добавили список адресов для выбора
			ViewData["MailToList"] = h.GetMailList();

			// по умолчанию выделен email пользователя
			var model = new SendReport();
			model.MailTo = new List<string>() { CurrentUser.Login };
			model.jobName = jobName;
			return View(model);
		}

		[HttpGet]
		public ActionResult GetReport(string jobName)
		{
			// вытащили отчет
			var jxml = DB.reportxml.SingleOrDefault(x => x.JobName == jobName);
			if (jxml == null)
				return Content("<p>Отчет не найден</p>");

			if (jxml.Xml.Length > 50000)
				return Content("<p>Отчет имеет слишком большой объем для отображения. Сохраните в Excel или отправьте на email</p>");

			// добавили отчет для вывода
			var model = new DataSet();
			model.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			return View(model);
		}

		/// <summary>
		/// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DisplayReport(SendReport model)
		{
			// вытащили отчет
			var jxml = DB.reportxml.Where(x => x.JobName == model.jobName).Select(x => x.JobName).SingleOrDefault();
			if (jxml == null)
			{
				logger.Error($"XML отчета {model.jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var jext = GetJobExtend(model.jobName);
			if (jext == null)
			{
				logger.Error($"Дополнительные параметры отчета {model.jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			// вытащили заголовок отчета
			ViewBag.Title = jext.CustomName;

			// добавили список адресов для выбора
			ViewData["MailToList"] = h.GetMailList();

			foreach (var error in model.Validate())
				ModelState.AddModelError(error.PropertyName, error.Message);

			// если модель невалидна - возвращаем пользователю
			if (!ModelState.IsValid)
				return View(model);

			var file = helper.GetExcel(jext);
			Mails.ManualPostReportMessage(jext, file.FullName, model.MailTo);
			SuccessMessage("Отчет отправлен на указанные email");
			return View(model);
		}

		/// <summary>
		/// Возвращает список поставщиков в заданных регионах. Указанные для исключения поставщики выделены
		/// </summary>
		/// <param name="RegionCodeEqual">Битовые маски регионов</param>
		/// <param name="SupplierIdNonEqual">Список поставшиков</param>
		/// <returns></returns>
		public JsonResult GetSupplierJson(List<decimal> RegionCodeEqual, List<long> SupplierIdNonEqual, List<long> SupplierIdEqual)
		{
			var supplierStringList = new List<string>();

			if (SupplierIdNonEqual != null)
				supplierStringList = SupplierIdNonEqual.Select(x => x.ToString()).ToList();
			else if (SupplierIdEqual != null)
				supplierStringList = SupplierIdEqual.Select(x => x.ToString()).ToList();

			return Json(new
			{
				results = (h.GetSupplierList(RegionCodeEqual).Select(x => new { value = x.Value, text = x.Text, selected = supplierStringList.Contains(x.Value) }))
			}, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Возвращает статус указанного задания и ссылку на последний отчет
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public JsonResult GetDisplayStatusJson(string jobName)
		{
			var jext = GetJobExtend(jobName);
			return Json(new
			{
				status = jext.DisplayStatus,
				url = Url.Action("DisplayReport", "Report", new { jobName = jobName })
			}, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Возвращает расширенную информацию о задании
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		protected jobextend GetJobExtend(string jobName)
		{
			return DB.jobextend.SingleOrDefault(x => x.JobName == jobName);
		}

		/// <summary>
		/// Заполняе модель указанного типа значениями, полученными в запросе
		/// </summary>
		/// <param name="model">Модель</param>
		/// <param name="type">Тип модели</param>
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

		/// <summary>
		/// Устанавливает значения ViewData, требующиеся для отображения формы ввода параметров заданной модели отчета
		/// </summary>
		/// <param name="model">Модель отчета</param>
		protected void SetViewData(Report model)
		{
			foreach (var item in model.ViewDataValues(h))
				ViewData[item.Key] = item.Value;
		}

		/// <summary>
		/// Устанавливает значения ViewData, требующиеся для отображения формы ввода параметров заданной модели триггера
		/// </summary>
		/// <param name="model">Модель триггера</param>
		protected void SetViewData(TriggerParam model)
		{
			foreach (var item in model.ViewDataValues(h))
				ViewData[item.Key] = item.Value;
		}
	}
}
