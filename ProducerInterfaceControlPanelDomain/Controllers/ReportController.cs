using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Data;
using System.IO;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Report;
using ProducerInterfaceCommon.Controllers;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class ReportController : BaseReportController
	{

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			TypeLoginUser = TypeUsers.ControlPanelUser;
			base.OnActionExecuting(filterContext);
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
			var schedulerName = GetSchedulerName();
			var query = cntx_.jobextendwithproducer.Where(x => x.SchedName == schedulerName);
			if (param.Enable.HasValue)
				query = query.Where(x => x.Enable == param.Enable);
			if (param.Producer.HasValue)
				query = query.Where(x => x.ProducerId == param.Producer);
			if (param.ReportType.HasValue)
				query = query.Where(x => x.ReportType == param.ReportType);
			if (!string.IsNullOrEmpty(param.ReportName))
				query = query.Where(x => x.CustomName.Contains(param.ReportName));
			if (param.RunFrom.HasValue)
				query = query.Where(x => x.LastRun >= param.RunFrom);
			if (param.RunTo.HasValue)
				query = query.Where(x => x.LastRun <= param.RunTo);

			var itemsCount = query.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
			var info = new SortingPagingInfo() { CurrentPageIndex = param.CurrentPageIndex, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };
			ViewBag.Info = info;

			var model = query.OrderByDescending(x => x.CreationDate).Skip(param.CurrentPageIndex * itemsPerPage).Take(itemsPerPage).ToList();
			return View(model);
		}

		[HttpGet]
		public ActionResult ReportDescription()
		{
			var model = cntx_.ReportDescription.ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult ReportDescription(int? id)
		{
			if (id.HasValue)
				return RedirectToAction("EditReportDescription", new { id = id.Value });

			ModelState.AddModelError("id", "Выберите тип отчета, который хотите изменить");
			var model = cntx_.ReportDescription.ToList();
			return View(model);
		}

		[HttpGet]
		public ActionResult EditReportDescription(int id)
		{
			var model = cntx_.ReportDescription.Single(x => x.Id == id);
			var regionCode = model.ReportRegion.Select(x => x.RegionCode).ToList();

			ViewData["Regions"] = cntx_.regionsnamesleaf
				.ToList()
				.OrderBy(x => x.RegionName)
				.Select(x => new SelectListItem { Value = x.RegionCode.ToString(), Text = x.RegionName, Selected = regionCode.Contains(x.RegionCode) })
				.ToList();

			var modelUI = new ReportDescriptionUI()
			{
				Id = model.Id,
				Name = model.Name,
				Description = model.Description,
				RegionList = regionCode
			};
			return View(modelUI);
		}

		[HttpPost]
		public ActionResult EditReportDescription(ReportDescriptionUI modelUI)
		{
			if (!ModelState.IsValid)
			{
				ViewData["Regions"] = cntx_.regionsnamesleaf
					.ToList()
					.OrderBy(x => x.RegionName)
					.Select(x => new SelectListItem { Value = x.RegionCode.ToString(), Text = x.RegionName, Selected = modelUI.RegionList.Contains(x.RegionCode) })
					.ToList();
				return View(modelUI);
			}

			var model = cntx_.ReportDescription.Single(x => x.Id == modelUI.Id);
			model.Description = modelUI.Description;
			model.ReportRegion.Clear();
			foreach (var regionCode in modelUI.RegionList)
				model.ReportRegion.Add(new ReportRegion() { RegionCode = regionCode, ReportId = modelUI.Id });

			cntx_.SaveChanges();
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
			var jext = cntx_.jobextend.Single(x => x.JobName == jobName);
			var producer = cntx_.producernames.SingleOrDefault(x => x.ProducerId == jext.ProducerId);
			var producerName = "удален";
			if (producer != null)
				producerName = producer.ProducerName;

			ViewBag.Title = $"История запусков отчета: \"{jext.CustomName}\", Производитель : \"{producerName}\"";
			var model = cntx_.reportrunlogwithuser.Where(x => x.JobName == jobName).OrderByDescending(x => x.RunStartTime).ToList();

			return View(model);
		}

		/// <summary>
		/// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult DisplayReport(string jobName)
		{
			var jxml = cntx_.reportxml.Single(x => x.JobName == jobName);
			ViewData["jobName"] = jobName;
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
			return View(ds);
		}

		public List<OptionElement> GetProducerList()
		{
			var schedulerName = GetSchedulerName();
			// возвращаем только производителей, у которых есть отчёты
			var producerIdList = cntx_.jobextend.Where(x => x.SchedName == schedulerName).Select(x => x.ProducerId).Distinct().ToList();
			var producers = cntx_.producernames.Where(x => producerIdList.Contains(x.ProducerId)).ToList()
				.Select(x => new OptionElement { Text = x.ProducerName, Value = x.ProducerId.ToString() }).ToList();

			var model = new List<OptionElement>() { new OptionElement { Text = "Все производители", Value = "" } };
			model.AddRange(producers);
			return model;
		}
	}
}