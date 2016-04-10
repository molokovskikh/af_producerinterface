using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Data;
using System.IO;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Report;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class ReportController : ProducerInterfaceCommon.Controllers.BaseReportController
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

			var itemCount = query.Count();
			var itemsOnPage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
			var skip = param.CurrentPageIndex * itemsOnPage;
			SetPager(itemCount, param.CurrentPageIndex, itemsOnPage);

			var model = query.OrderBy(x => x.CreationDate).Skip(skip).Take(itemsOnPage).ToList();
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
				return RedirectToAction("EditReportDescription", new {id = id.Value});

			ModelState.AddModelError("id", "Выберите тип отчета, который хотите изменить");
			var model = cntx_.ReportDescription.ToList();
			return View(model);
		}

		[HttpGet]
		public ActionResult EditReportDescription(int id)
		{
			ViewData["Regions"] = cntx_.regionsnamesleaf
				.ToList()
				.OrderBy(x => x.RegionName)
				.Select(x => new OptionElement { Value = x.RegionCode.ToString(), Text = x.RegionName })
				.ToList();

			var model = cntx_.ReportDescription.Single(x => x.Id == id);
			var modelUI = new ReportDescriptionUI() {
				Id = model.Id,
				Name = model.Name,
				Description = model.Description,
				RegionList = model.ReportRegion.Select(x => x.RegionCode).ToList()
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
					.Select(x => new OptionElement { Value = x.RegionCode.ToString(), Text = x.RegionName })
					.ToList();
				return View(modelUI);
			}
			var model = cntx_.ReportDescription.Single(x => x.Id == modelUI.Id);
			model.Description = modelUI.Description;
			var rr = new List<ReportRegion>();
			foreach (var r in modelUI.RegionList)
				rr.Add(new ReportRegion() { RegionCode = r, ReportId = modelUI.Id });

			foreach (var child in model.ReportRegion.ToList())
				model.ReportRegion.Remove(child);
			cntx_.SaveChanges();

			model.ReportRegion = rr;
      cntx_.SaveChanges();
			SuccessMessage("Свойства отчета сохранены");
			return RedirectToAction("ReportDescription");
		}

		private void SetPager(int itemCount, int page, int itemsOnPage)
		{
			var info = new SortingPagingInfo();
			info.CurrentPageIndex = page;
			info.PageCount = (int)Math.Ceiling((decimal)itemCount / itemsOnPage);
			ViewBag.Info = info;
		}

		/// <summary>
		/// Возвращает историю запуска отчета
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public ActionResult RunHistory(string jobName)
		{
			var reportName = cntx_.jobextend.Single(x => x.JobName == jobName).CustomName;
			var ProducerId = cntx_.jobextend.Where(y => y.JobName == jobName).First().ProducerId;
			var ProducerName = cntx_.producernames.Where(x => x.ProducerId == ProducerId).First().ProducerName;

			ViewBag.Title = $"История запусков отчета: \"{reportName}\", Производитель : \"{ProducerName}\"";
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
			var jxml = cntx_.reportxml.SingleOrDefault(x => x.JobName == jobName);
			if (jxml == null)
				return View("Error", (object)"Отчет не найден");

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