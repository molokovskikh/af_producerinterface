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
				return View("Error", (object)(e.Message));
			}

			var userName = cntx.Account.Single(x => x.Id == userId).Name;
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
			cntx.SaveChanges(CurrentUser, "Добавление отчета");
			return RedirectToAction("JobList", "Report");
			//return View("Success", (object)"Отчет успешно добавлен");
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
			cntx.SaveChanges(CurrentUser, "Удаление отчета");
			return View("Success", (object)"Задача удалена");
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
			cntx.SaveChanges(CurrentUser, "Восстановление отчета");
			return View("Success", (object)"Задача восстановлена");
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
			cntx.SaveChanges(CurrentUser, "Редактирование отчета");
			return View("Success", (object)"Отчет успешно изменён");
		}

		/// <summary>
		/// Выводит список заданий производителя
		/// </summary>
		/// <returns></returns>
		public ActionResult JobList()
		{
			var scheduler = GetScheduler();
			var jobList = cntx.jobextend.Where(x => x.ProducerId == producerId
																				&& x.SchedName == scheduler.SchedulerName
																				&& x.Enable == true)
																				.OrderByDescending(x => x.CreationDate)
																				.ToList();
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
				return View("Error", (object)"Задача не найдена");

			var jext = GetJobExtend(jobName, jobGroup);
			if (jext == null)
				return View("Error", (object)"Дополнительные параметры задачи не найдены");

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Запуск \"{param.CastomName}\"";

			RunNowParam model;
			if (param is IInterval)
				model = new RunNowIntervalParam();
			// TODO при появлении неинтервальных отчетов добавить код
			else
				return View("Error", (object)"Отчет этого типа невозможно запустить вручную");

			// при GET возвращаем пустую модель для заполнения
			if (HttpContext.Request.HttpMethod == "GET") {
				SetViewData(model);
				return View(model);
			}

			// биндим
			Bind(model, model.GetType());

			model.UserId = userId;

			foreach (var error in model.Validate())
				ModelState.AddModelError(error.PropertyName, error.Message);

			// если модель невалидна - возвращаем пользователю
			if (!ModelState.IsValid) {
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
				return View("Error", (object)(e.Message));
			}

			// отправили статус, что отчет готовится
			jext.DisplayStatusEnum = DisplayStatus.Processed;
			jext.LastRun = DateTime.Now;
			cntx.SaveChanges();

			var message = "АналитФармация приступила к формированию запрошенного отчета. Как только отчет будет готов, он сразу же ";
			if (model.ByDisplay)
				message = message + "появится в списке отчетов";
			if (model.ByDisplay && model.ByEmail)
				message = message + " и ";
			if (model.ByEmail)
				message = message + "будет отправлен на указанные адреса электронной почты";

			return View("Success", (object)message);
		}


		/// <summary>
		/// Возвращает историю запуска отчёта
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
				return View("Error", (object)"Задача не найдена");

			var jext = GetJobExtend(key.Name, key.Group);
			if (jext == null)
				return View("Error", (object)"Дополнительные параметры задачи не найдены");

			var param = (Report)job.JobDataMap["param"];
			ViewBag.Title = $"Расписание для \"{param.CastomName}\"";

			CronParam model;
			if (param is IInterval)
				model = new CronIntervalParam();
			// TODO при появлении неинтервальных отчетов добавить код
			else
				return View("Error", (object)"Отчет этого типа невозможно запустить вручную");

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
			if (!ModelState.IsValid) {
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
				return View("Error", (object)(e.Message));
			}
			// меняем человекочитаемое описание в доп. параметрах задачи
			jext.Scheduler = model.CronHumanText;
			cntx.SaveChanges(CurrentUser, "Установка расписания отчета");
			return View("Success", (object)"Расписание успешно установлено");
		}

		/// <summary>
		/// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public ActionResult DisplayReport(string jobName, string jobGroup)
		{
			var jxml = cntx.reportxml.SingleOrDefault(x => x.JobName == jobName && x.JobGroup == jobGroup);
			if (jxml == null)
				return View("Error", (object)"Отчет не найден");

			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
			return View(ds);
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
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public JsonResult GetDisplayStatusJson(string jobName, string jobGroup)
		{
			var jext = GetJobExtend(jobName, jobGroup);
			return Json(new
			{
				status = jext.DisplayStatus,
				url = Url.Action("DisplayReport", "Report", new { jobName = jobName, jobGroup = jobGroup })
			}, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Возвращает расширенную информацию о задании
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		protected jobextend GetJobExtend(string jobName, string jobGroup)
		{
			var scheduler = GetScheduler();
			return cntx.jobextend.SingleOrDefault(x => x.SchedName == scheduler.SchedulerName
																																													&& x.JobName == jobName
																																													&& x.JobGroup == jobGroup
																																													&& x.ProducerId == producerId
																																													&& x.Enable == true);
		}

		public JsonResult GetCatalogDragFamalyNames(string term)
		{
			var ret = Json(h.GetSearchCatalogFamalyName(term).ToList().Select(x => new { value = x.Value, text = x.Text }), JsonRequestBehavior.AllowGet);
			return ret;
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
				throw new NotSupportedException("Должен использоваться удалённый ServerScheduler");

			return scheduler;
		}
	}
}
