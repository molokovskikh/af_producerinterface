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
	public class ReportController : MasterBaseController
	{        // GET: Report

		private string shedName_ = ""; // DebugShedulerName(); 

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			// ViewBag.currentUser = User.Identity.Name;
			shedName_ = DebugShedulerName();
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
			var query = cntx_.jobextendwithproducer.Where(x => x.SchedName == shedName_ && x.Enable == param.Enable);
			if (param.Producer.HasValue)
				query = query.Where(x => x.ProducerId == param.Producer);

			var itemCount = query.Count();
			var itemsOnPage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
			var skip = param.CurrentPageIndex * itemsOnPage;
			SetPager(itemCount, param.CurrentPageIndex, itemsOnPage);

			var model = query.OrderBy(x => x.CreationDate).Skip(skip).Take(itemsOnPage).ToList();
			return View(model);
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
			ViewData["repName"] = $"История запусков отчета \"{reportName}\"";
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

			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
			return View(ds);
		}


		public List<OptionElement> GetProducerList()
		{
			// возвращаем только производителей, у которых есть отчёты
			var producerIdList = cntx_.jobextend.Where(x => x.SchedName == shedName_).Select(x => x.ProducerId).Distinct().ToList();
			var producers = cntx_.producernames.Where(x => producerIdList.Contains(x.ProducerId)).ToList()
				.Select(x => new OptionElement { Text = x.ProducerName, Value = x.ProducerId.ToString() }).ToList();

			var model = new List<OptionElement>() { new OptionElement { Text = "(Ничего не выбрано)", Value = "" } };
			model.AddRange(producers);
			return model;
		}

	}
}