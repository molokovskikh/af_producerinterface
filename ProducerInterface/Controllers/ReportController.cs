using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using Quartz;
using Quartz.Impl;
using ProducerInterfaceCommon.Heap;
using System.Configuration;
using Common.Logging;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using System.Data;
using System.IO;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterface.Controllers
{
	public class ReportController : MasterBaseController
	{
		//
		// GET: /Report/

		protected static readonly Common.Logging.ILog logger = LogManager.GetLogger(typeof(ReportController));

		protected ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx;
		protected NamesHelper h;
		protected long userId;
		protected long producerId;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
			//  TODO: берётся у юзера            
			try
			{
				userId = CurrentUser.ID_LOG;
				producerId = (long)CurrentUser.AccountCompany.ProducerId;
			}
			catch
			{
				// Ignore
			}

			h = new NamesHelper(cntx, userId);
		}

		private IScheduler GetScheduler()
		{
#if DEBUG
			return GetDebagSheduler();
#else
						return GetRemoteSheduler();
#endif
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
				return RedirectToAction("JobList", "Report");
			}

			// получили пустую модель нужного типа
			var type = GetModelType(Id.Value);
			var model = (Report)Activator.CreateInstance(type);
			model.Id = Id.Value;
			model.ProducerId = producerId;
			//model.MailSubject = h.GetMailOkReportSubject();

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

			var scheduler = GetScheduler();
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
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}

			// иначе - успех
			var jext = new jobextend()
			{
				CreationDate = DateTime.Now,
				CreatorId = userId,
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
			cntx.SaveChanges(CurrentUser, "Добавление отчета");
			SuccessMessage("Отчет успешно добавлен");
			return RedirectToAction("JobList", "Report");
		}

		/// <summary>
		/// Ставит задание на паузу в Quartz, в расширенных параметрах задания снимает флаг Enable
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult Delete(string jobName, string jobGroup)
		{
			var key = new JobKey(jobName, jobGroup);
			var scheduler = GetScheduler();
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				ErrorMessage("Задача не найдена");
				return RedirectToAction("JobList", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				ErrorMessage("Дополнительные параметры задачи не найдены");
				return RedirectToAction("JobList", "Report");
			}

			try
			{
				scheduler.PauseJob(key);
			}
			catch (Exception e)
			{
				logger.Error($"Job {key.Name} {key.Group} paused failed:" + e.Message, e);
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}
			jext.Enable = false;
			cntx.SaveChanges(CurrentUser, "Удаление отчета");
			SuccessMessage("Задача удалена");
			return RedirectToAction("JobList", "Report");

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
			var scheduler = GetScheduler();
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				ErrorMessage("Задача не найдена");
				return RedirectToAction("JobList", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				ErrorMessage("Дополнительные параметры задачи не найдены");
				return RedirectToAction("JobList", "Report");
			}

			try
			{
				scheduler.ResumeJob(key);
			}
			catch (Exception e)
			{
				logger.Error($"Job {key.Name} {key.Group} resume failed:" + e.Message, e);
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}
			jext.Enable = true;
			cntx.SaveChanges(CurrentUser, "Восстановление отчета");
			SuccessMessage("Задача восстановлена");
			return RedirectToAction("JobList", "Report");
		}

		/// <summary>
		/// Возвращает параметры отчета, сохранённые в указанном задании, для редактирования, проверяет их, сохраняет изменения
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult Edit(string jobName, string jobGroup)
		{
			var key = new JobKey(jobName, jobGroup);
			var scheduler = GetScheduler();
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				ErrorMessage("Задача не найдена");
				return RedirectToAction("JobList", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				ErrorMessage("Дополнительные параметры задачи не найдены");
				return RedirectToAction("JobList", "Report");
			}

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
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}

			// вносим изменения в расширенные параметры
			jext.LastModified = DateTime.Now;
			jext.CustomName = model.CastomName;
			cntx.SaveChanges(CurrentUser, "Редактирование отчета");
			SuccessMessage("Отчет успешно изменен");
			return RedirectToAction("JobList", "Report");
		}

		/// <summary>
		/// Выводит список заданий производителя
		/// </summary>
		/// <returns></returns>
		public ActionResult JobList(long? cid)
		{
			var scheduler = GetScheduler();
			// вытащили всех создателей, создававших отчеты этого производителя
			var creatorIds = cntx.jobextend
				.Where(x => x.ProducerId == producerId && x.SchedName == scheduler.SchedulerName && x.Enable == true)
				.Select(x => x.CreatorId)
				.Distinct().ToList();
			ViewData["creators"] = cntx.Account.Where(x => creatorIds.Contains(x.Id)).
				Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() }).ToList();

			var query = cntx.jobextendwithproducer.Where(x => x.ProducerId == producerId
																												&& x.SchedName == scheduler.SchedulerName
																												&& x.Enable == true);
			if (cid.HasValue)
				query = query.Where(x => x.CreatorId == cid);

			var jobList = query.OrderByDescending(x => x.CreationDate).ToList();
			return View(jobList);
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
			var scheduler = GetScheduler();
			// нашли задачу
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				ErrorMessage("Задача не найдена");
				return RedirectToAction("JobList", "Report");
			}

			var jext = GetJobExtend(jobName);
			if (jext == null)
			{
				ErrorMessage("Дополнительные параметры задачи не найдены");
				return RedirectToAction("JobList", "Report");
			}

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Запуск \"{param.CastomName}\"";

			RunNowParam model;
			if (param is IInterval)
				model = new RunNowIntervalParam();
			// TODO при появлении неинтервальных отчетов добавить код
			else
			{
				ErrorMessage("Отчет этого типа невозможно запустить вручную");
				return RedirectToAction("JobList", "Report");
			}

			// при GET возвращаем пустую модель для заполнения
			if (HttpContext.Request.HttpMethod == "GET")
			{
				SetViewData(model);
				return View(model);
			}

			// биндим
			Bind(model, model.GetType());

			model.UserId = userId;

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

			// записали в историю запусков
			var reportRunLog = new ReportRunLog() { AccountId = userId, Ip = CurrentUser.IP, JobName = key.Name, RunNow = true };
			cntx.ReportRunLog.Add(reportRunLog);
			cntx.SaveChanges();

			try
			{
				scheduler.ScheduleJob(trigger);
			}
			catch (Exception e)
			{
				logger.Error($"Job {key.Name} {key.Group} run now failed:" + e.Message, e);
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}

			// отправили статус, что отчет готовится
			jext.DisplayStatusEnum = DisplayStatus.Processed;
			jext.LastRun = DateTime.Now;
			cntx.SaveChanges();

			var message = "АналитФармация приступила к формированию запрошенного отчета. Как только отчет будет готов, он сразу же появится в списке отчетов";
			if (model.MailTo != null && model.MailTo.Count > 0)
				message = message + " и будет отправлен на указанные email";

			SuccessMessage(message);
			return RedirectToAction("JobList", "Report");
		}


		/// <summary>
		/// Возвращает историю запуска отчета
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult RunHistory(string jobName)
		{
			ViewData["repName"] = $"История запусков отчета \"{h.GetReportName(jobName)}\"";
			var model = cntx.reportrunlogwithuser.Where(x => x.JobName == jobName).OrderByDescending(x => x.RunStartTime).ToList();
			return View(model);
		}

		/// <summary>
		/// Возвращает форму для заполнения параметров запуска отчетов, включая расписание, устанавливает исполнение задания по расписанию
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult ScheduleJob(string jobName, string jobGroup)
		{
			var key = JobKey.Create(jobName, jobGroup);
			var scheduler = GetScheduler();
			// нашли задачу
			var job = scheduler.GetJobDetail(key);
			if (job == null)
			{
				ErrorMessage("Задача не найдена");
				return RedirectToAction("JobList", "Report");
			}

			var jext = GetJobExtend(key.Name);
			if (jext == null)
			{
				ErrorMessage("Дополнительные параметры задачи не найдены");
				return RedirectToAction("JobList", "Report");
			}

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Расписание для \"{param.CastomName}\"";

			CronParam model;
			if (param is IInterval)
				model = new CronIntervalParam();
			// TODO при появлении неинтервальных отчетов добавить код
			else
			{
				ErrorMessage("Для отчета этого невозможно задать расписание");
				return RedirectToAction("JobList", "Report");
			}

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

			model.UserId = userId;

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
				logger.Error($"Job {key.Name} {key.Group} run now failed:" + e.Message, e);
				ErrorMessage(e.Message);
				return RedirectToAction("JobList", "Report");
			}
			// меняем человекочитаемое описание в доп. параметрах задачи
			jext.Scheduler = model.CronHumanText;
			cntx.SaveChanges(CurrentUser, "Установка расписания отчета");
			SuccessMessage("Расписание успешно установлено");
			return RedirectToAction("JobList", "Report");
		}

		/// <summary>
		/// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult DisplayReport(SendReport model)
		{
			// вытащили отчет
			var jxml = cntx.reportxml.SingleOrDefault(x => x.JobName == model.jobName);
			if (jxml == null)
			{
				ErrorMessage("Отчет не найден");
				return RedirectToAction("JobList", "Report");
			}
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			// вытащили заголовок отчета
			ViewBag.Title = ds.Tables["Titles"].Rows[0][0].ToString();
			// добавили список адресов для выбора
			ViewData["MailTo"] = h.GetMailList();
			// добавили отчет для вывода
			ViewData["ds"] = ds;

			// если указаны email - проверяем
			if (model.MailTo != null && model.MailTo.Count > 0)
			{
				var ea = new EmailAddressAttribute();
				var ok = true;
				foreach (var mail in model.MailTo)
					ok = ok && ea.IsValid(mail);
				if (!ok)
					ModelState.AddModelError("MailTo", "Неверный формат email");
			}

			// если POST и модель валидна - отправляем
			if (HttpContext.Request.HttpMethod == "POST" && ModelState.IsValid)
			{
				var jext = cntx.jobextend.Single(x => x.JobName == model.jobName);
				var file = GetExcel(jext);
				EmailSender.ManualPostReportMessage(cntx, userId, jext, file.FullName, model.MailTo);
				SuccessMessage("Отчет отправлен на указанные email");
			}
			return View(model);
		}

		/// <summary>
		/// Возвращает список поставщиков в заданных регионах. Указанные для исключения поставщики выделены
		/// </summary>
		/// <param name="RegionCodeEqual">Битовые маски регионов</param>
		/// <param name="SupplierIdNonEqual">Список поставшиков</param>
		/// <returns></returns>
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
			return cntx.jobextend.SingleOrDefault(x => x.JobName == jobName);
		}

		public JsonResult GetCatalogDragFamalyNames(string term)
		{
			var ret = Json(h.GetSearchCatalogFamalyName(term).ToList().Select(x => new { value = x.Value, text = x.Text }), JsonRequestBehavior.AllowGet);
			return ret;
		}

		/// <summary>
		/// Возвращает отчет в виде excel-файла
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public FileResult GetFile(string jobName)
		{
			var jext = cntx.jobextend.Single(x => x.JobName == jobName);
			var file = GetExcel(jext);

			// вернули файл
			byte[] fileBytes = System.IO.File.ReadAllBytes(file.FullName);
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			return File(fileBytes, contentType, file.Name);
		}

		private FileInfo GetExcel(jobextend jext)
		{
			var jxml = cntx.reportxml.Single(x => x.JobName == jext.JobName);

			// вытащили сохраненный отчет
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			// создали процессор для этого типа отчетов
			var type = GetModelType(jext.ReportType);
			var report = (Report)Activator.CreateInstance(type);
			var processor = report.GetProcessor();

			// создали excel-файл
			return processor.CreateExcel(jext.JobGroup, jext.JobName, ds);
		}

		/// <summary>
		/// Возвращает тип отчета по идентификатору типа отчета
		/// </summary>
		/// <param name="id">Идентификатор типа отчета</param>
		/// <returns></returns>
		protected Type GetModelType(int id)
		{
			var typeName = ((Reports)id).ToString();
			var type = Type.GetType($"ProducerInterfaceCommon.Models.{typeName}, {typeof(Report).Assembly.FullName}");
			if (type == null)
				throw new NotSupportedException($"Не удалось создать тип {typeName} по идентификатору {id}");
			return type;
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


		//	logger.Debug($"Start setting job {model.JobGroup} {model.JobName}");

		/// <summary>
		/// Возвращает ссылку на локальный шедулер (запускаемый одновременно с сайтом на той же машине)
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Возвращает удалённый шедулер (инсталлированный отдельно как win-служба)
		/// </summary>
		/// <returns></returns>
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
				throw new NotSupportedException("Должен использоваться удаленный ServerScheduler");

			return scheduler;
		}
	}
}
