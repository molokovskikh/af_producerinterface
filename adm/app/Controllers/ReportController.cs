using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Data;
using System.IO;
using Common.Logging;
using NHibernate.Linq;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Report;
using ProducerInterfaceCommon.Helpers;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;
using Quartz;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class ReportController : BaseController
	{
		private ReportHelper helper;
		protected static readonly ILog logger = LogManager.GetLogger(typeof (ReportController));
		protected NamesHelper h;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			helper = new ReportHelper(DB);
			h = new NamesHelper(CurrentUser.Id);
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

		[HttpGet]
		public ActionResult Index()
		{
			var model = new SearchProducerReportsModel();
			model.Enable = true;
			model.CurrentPageIndex = 0;

			ViewBag.ProducerList = GetProducerList();
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(SearchProducerReportsModel model)
		{
			ViewBag.ProducerList = GetProducerList();
			return View(model);
		}

		public ActionResult SearchResult(SearchProducerReportsModel param)
		{
			var schedulerName = ReportHelper.GetSchedulerName();
			var query = DbSession.Query<Job>().Fetch(x => x.Owner).Fetch(x => x.Producer)
				.Where(x => x.SchedName == schedulerName);
			if (param.Enable.HasValue)
				query = query.Where(x => x.Enable == param.Enable);
			if (param.Producer.HasValue)
				query = query.Where(x => x.Producer.Id == param.Producer);
			if (param.ReportType.HasValue)
				query = query.Where(x => (int) x.ReportType == param.ReportType);
			if (!string.IsNullOrEmpty(param.ReportName))
				query = query.Where(x => x.CustomName.Contains(param.ReportName));
			if (param.RunFrom.HasValue)
				query = query.Where(x => x.LastRun >= param.RunFrom);
			if (param.RunTo.HasValue)
				query = query.Where(x => x.LastRun <= param.RunTo);

			var itemsCount = query.Count();
			var itemsPerPage = Convert.ToInt32(ConfigurationManager.AppSettings["ReportCountPage"]);
			var info = new SortingPagingInfo() {
				CurrentPageIndex = param.CurrentPageIndex,
				ItemsCount = itemsCount,
				ItemsPerPage = itemsPerPage
			};
			ViewBag.Info = info;

			var model =
				query.OrderByDescending(x => x.CreationDate).Skip(param.CurrentPageIndex*itemsPerPage).Take(itemsPerPage).ToList();

			ViewData["DeleteOldReportsTerm"] = int.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
			return View(model);
		}

		[HttpGet]
		public ActionResult ReportDescription()
		{
			var model = DB.ReportDescription.ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult ReportDescription(int? id)
		{
			if (id.HasValue)
				return RedirectToAction("EditReportDescription", new {id = id.Value});

			ModelState.AddModelError("id", "Выберите тип отчета, который хотите изменить");
			var model = DB.ReportDescription.ToList();
			return View(model);
		}

		[HttpGet]
		public ActionResult EditReportDescription(int id)
		{
			var model = DB.ReportDescription.Single(x => x.Id == id);
			var regionCode = model.ReportRegion.Select(x => x.RegionId).ToList();

			ViewData["Regions"] = RegionsSelect(regionCode);

			var modelUI = new ReportDescriptionUI() {
				Id = model.Id,
				Name = model.Name,
				Description = model.Description,
				RegionList = regionCode
			};
			return View(modelUI);
		}

		private object RegionsSelect(IEnumerable<ulong> selected)
		{
			return DB.Regions()
				.Select(x => new SelectListItem {Value = x.Id.ToString(), Text = x.Name, Selected = selected.Contains(x.Id)})
				.ToList();
		}

		[HttpPost]
		public ActionResult EditReportDescription(ReportDescriptionUI modelUI)
		{
			if (!ModelState.IsValid) {
				ViewData["Regions"] = RegionsSelect(modelUI.RegionList);
				return View(modelUI);
			}

			var model = DB.ReportDescription.Single(x => x.Id == modelUI.Id);
			model.Description = modelUI.Description;
			model.ReportRegion.Clear();
			foreach (var regionCode in modelUI.RegionList)
				model.ReportRegion.Add(new ReportRegion() {RegionId = regionCode, ReportId = modelUI.Id});

			DB.SaveChanges();
			SuccessMessage("Свойства отчета сохранены");
			return RedirectToAction("ReportDescription");
		}

		/// <summary>
		/// Возвращает историю запуска отчета
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult RunHistory(string jobName)
		{
			var jext = DB.jobextend.Single(x => x.JobName == jobName);
			var producerName = DB.producernames.SingleOrDefault(x => x.ProducerId == jext.ProducerId)?.ProducerName
				?? "<отсутсвует>";

			ViewBag.Title = $"История запусков отчета: \"{jext.CustomName}\", Производитель : \"{producerName}\"";
			var model = DB.reportrunlogwithuser.Where(x => x.JobName == jobName).OrderByDescending(x => x.RunStartTime).ToList();

			return View(model);
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

		/// <summary>
		/// Заполняе модель указанного типа значениями, полученными в запросе
		/// </summary>
		/// <param name="model">Модель</param>
		/// <param name="type">Тип модели</param>
		protected void Bind(object model, Type type)
		{
			this.Binders.GetBinder(type).BindModel(this.ControllerContext, new ModelBindingContext() {
				ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType((Func<object>) (() => model), type),
				ModelName = (string) null,
				ModelState = this.ModelState,
				PropertyFilter = (Predicate<string>) (propertyName => true),
				ValueProvider = this.ValueProvider
			});
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
			if (jext == null) {
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			if (jext.CreatorId != CurrentUser.Id && !CurrentUser.IsAdmin) {
				ErrorMessage("Вы можете редактировать только свои отчеты");
				return RedirectToAction("Index", "Report");
			}

			// нашли задачу
			var scheduler = helper.GetScheduler();
			var key = JobKey.Create(jobName, jobGroup);
			var job = scheduler.GetJobDetail(key);
			if (job == null) {
				logger.Error($"Отчет {jobName} не найден");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			var param = (Report) job.JobDataMap["param"];
			ViewBag.Title = $"Установка времени формирования для отчета \"{param.CastomName}\"";

			CronParam model;
			if (param is IInterval)
				model = new CronIntervalParam();
			else
				model = new CronNotIntervalParam();

			// триггер вставлялся с идентификатором задачи
			var oldTriggerKey = new TriggerKey(key.Name, key.Group);
			// используются только cron-триггеры
			var oldTrigger = (ICronTrigger) (scheduler.GetTrigger(oldTriggerKey));

			// при GET возвращаем пустую модель для заполнения
			if (HttpContext.Request.HttpMethod == "GET") {
				//// если триггер уже был - устанавливаем UI его значением
				if (oldTrigger != null)
					model = (CronParam) oldTrigger.JobDataMap["tparam"];
				SetViewData(model);
				return View(model);
			}

			// биндим
			Bind(model, model.GetType());

			model.UserId = CurrentUser.Id;

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

			try {
				if (oldTrigger == null)
					scheduler.ScheduleJob(trigger);
				else
					scheduler.RescheduleJob(oldTriggerKey, trigger);
			} catch (Exception e) {
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

		/// <summary>
		/// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult DisplayReport(string jobName)
		{
			var jxml = DB.reportxml.Single(x => x.JobName == jobName);
			ViewData["jobName"] = jobName;
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
			return View(ds);
		}

		public List<OptionElement> GetProducerList()
		{
			var schedulerName = ReportHelper.GetSchedulerName();
			// возвращаем только производителей, у которых есть отчёты
			var producerIdList = DB.jobextend.Where(x => x.SchedName == schedulerName && x.ProducerId != null)
				.Select(x => x.ProducerId).Distinct().ToList();
			var producers = DB.producernames.Where(x => producerIdList.Contains(x.ProducerId)).ToList()
				.Select(x => new OptionElement {Text = x.ProducerName, Value = x.ProducerId.ToString()}).ToList();

			var model = new List<OptionElement>() {new OptionElement {Text = "Все производители", Value = ""}};
			model.AddRange(producers);
			return model;
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
			if (jext == null) {
				logger.Error($"Дополнительные параметры отчета {jobName} не найдены");
				ErrorMessage("Отчет не найден");
				return RedirectToAction("Index", "Report");
			}

			if (jext.CreatorId != CurrentUser.Id && !CurrentUser.IsAdmin) {
				ErrorMessage("Вы можете удалять только свои отчеты");
				return RedirectToAction("Index", "Report");
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
			SuccessMessage("Отчет удален");
			return RedirectToAction("Index", "Report");
		}
	}
}