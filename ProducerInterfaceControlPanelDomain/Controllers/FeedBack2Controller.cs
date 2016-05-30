using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Controllers;
using ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class FeedBack2Controller : BaseReportController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			TypeLoginUser = TypeUsers.ControlPanelUser;
			base.OnActionExecuting(filterContext);
		}

		[HttpGet]
		public ActionResult Index()
		{
			var prDictionary = GetProducerList();
			var model = new FeedBackFilter2()
			{
				ProducerList = GetProducerListUi(prDictionary),
				AccountList = GetAccountList(),
				DateBegin = GetMinDate(),
				DateEnd = GetMaxDate(),
				ItemsPerPage = 50,
				ItemsPerPageList = GetItemsPerPageList(50),
				CurrentPageIndex = 0,
				StatusList = GetStatusList()
			};
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(FeedBackFilter2 model)
		{
			var prDictionary = GetProducerList();
			model.ProducerList = GetProducerListUi(prDictionary, model.ProducerId);
			model.AccountList = GetAccountList(model.AccountId);
			model.ItemsPerPageList = GetItemsPerPageList(model.ItemsPerPage);
			model.StatusList = GetStatusList(model.Status);
			return View(model);
		}

		public ActionResult SearchResult(FeedBackFilter2 filter)
		{
			var query = cntx_.AccountFeedBack.AsQueryable();
			if (filter.DateBegin.HasValue)
				query = query.Where(x => x.DateAdd >= filter.DateBegin.Value);
			if (filter.DateEnd.HasValue) {
				var dateEnd = filter.DateEnd.Value.AddDays(1);
				query = query.Where(x => x.DateAdd <= dateEnd);
			}
			if (filter.AccountId.HasValue)
				query = query.Where(x => x.AccountId == filter.AccountId);
			if (filter.ProducerId.HasValue)
				query = query.Where(x => x.Account.AccountCompany.ProducerId == filter.ProducerId);
			if (filter.Status.HasValue)
				query = query.Where(x => x.Status == filter.Status);

			var itemsCount = query.Count();
			var info = new SortingPagingInfo() { CurrentPageIndex = filter.CurrentPageIndex, ItemsCount = itemsCount, ItemsPerPage = filter.ItemsPerPage };
			ViewBag.Info = info;
			ViewBag.PrDictionary = GetProducerList();

			var model = query.OrderByDescending(x => x.DateAdd).Skip(filter.CurrentPageIndex * filter.ItemsPerPage).Take(filter.ItemsPerPage).ToList();
			return View(model);
		}

		//[HttpGet]
		//public ActionResult ReportDescription()
		//{
		//	var model = cntx_.ReportDescription.ToList();
		//	return View(model);
		//}

		//[HttpPost]
		//public ActionResult ReportDescription(int? id)
		//{
		//	if (id.HasValue)
		//		return RedirectToAction("EditReportDescription", new { id = id.Value });

		//	ModelState.AddModelError("id", "Выберите тип отчета, который хотите изменить");
		//	var model = cntx_.ReportDescription.ToList();
		//	return View(model);
		//}

		//[HttpGet]
		//public ActionResult EditReportDescription(int id)
		//{
		//	var model = cntx_.ReportDescription.Single(x => x.Id == id);
		//	var regionCode = model.ReportRegion.Select(x => x.RegionCode).ToList();

		//	ViewData["Regions"] = cntx_.regionsnamesleaf
		//		.ToList()
		//		.OrderBy(x => x.RegionName)
		//		.Select(x => new SelectListItem { Value = x.RegionCode.ToString(), Text = x.RegionName, Selected = regionCode.Contains(x.RegionCode) })
		//		.ToList();

		//	var modelUI = new ReportDescriptionUI()
		//	{
		//		Id = model.Id,
		//		Name = model.Name,
		//		Description = model.Description,
		//		RegionList = regionCode
		//	};
		//	return View(modelUI);
		//}

		//[HttpPost]
		//public ActionResult EditReportDescription(ReportDescriptionUI modelUI)
		//{
		//	var model = cntx_.ReportDescription.Single(x => x.Id == modelUI.Id);
		//	var regionCode = model.ReportRegion.Select(x => x.RegionCode).ToList();

		//	if (!ModelState.IsValid)
		//	{
		//		ViewData["Regions"] = cntx_.regionsnamesleaf
		//			.ToList()
		//			.OrderBy(x => x.RegionName)
		//			.Select(x => new SelectListItem { Value = x.RegionCode.ToString(), Text = x.RegionName, Selected = regionCode.Contains(x.RegionCode) })
		//			.ToList();
		//		return View(modelUI);
		//	}
		//	model.Description = modelUI.Description;
		//	var rr = new List<ReportRegion>();
		//	foreach (var r in modelUI.RegionList)
		//		rr.Add(new ReportRegion() { RegionCode = r, ReportId = modelUI.Id });

		//	foreach (var child in model.ReportRegion.ToList())
		//		model.ReportRegion.Remove(child);
		//	cntx_.SaveChanges();

		//	model.ReportRegion = rr;
		//	cntx_.SaveChanges();
		//	SuccessMessage("Свойства отчета сохранены");
		//	return RedirectToAction("ReportDescription");
		//}

		///// <summary>
		///// Возвращает историю запуска отчета
		///// </summary>
		///// <param name="jobName">Имя задания в Quartz</param>
		///// <returns></returns>
		//public ActionResult RunHistory(string jobName)
		//{
		//	var reportName = cntx_.jobextend.Single(x => x.JobName == jobName).CustomName;
		//	var ProducerId = cntx_.jobextend.Where(y => y.JobName == jobName).First().ProducerId;
		//	var ProducerName = cntx_.producernames.Where(x => x.ProducerId == ProducerId).First().ProducerName;

		//	ViewBag.Title = $"История запусков отчета: \"{reportName}\", Производитель : \"{ProducerName}\"";
		//	var model = cntx_.reportrunlogwithuser.Where(x => x.JobName == jobName).OrderByDescending(x => x.RunStartTime).ToList();

		//	return View(model);
		//}

		///// <summary>
		///// Возвращает последнюю версию указанного отчета для отображения пользователю в веб-интерфейсе
		///// </summary>
		///// <param name="jobName">Имя задания в Quartz</param>
		///// <returns></returns>
		//public ActionResult DisplayReport(string jobName)
		//{
		//	var jxml = cntx_.reportxml.SingleOrDefault(x => x.JobName == jobName);
		//	if (jxml == null)
		//		return View("Error", (object)"Отчет не найден");

		//	ViewData["jobName"] = jobName;
		//	var ds = new DataSet();
		//	ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);
		//	return View(ds);
		//}

		private List<SelectListItem> GetStatusList(int? status = null)
		{
				return new List<SelectListItem> {
					new SelectListItem { Text = "Все обращения", Value = "" },
					new SelectListItem { Text = "Обработанные", Value = ((int)FeedBackStatus.Processed).ToString(), Selected = status == (int)FeedBackStatus.Processed },
					new SelectListItem { Text = "Необработанные", Value = ((int)FeedBackStatus.New).ToString(), Selected = status == (int)FeedBackStatus.New },
				};
		}

		// возвращает список производителей, от пользователей которых есть сообщения в обратной связи
		private Dictionary<long, string> GetProducerList()
		{
			var cIds = cntx_.AccountFeedBack.Where(x => x.AccountId.HasValue).Select(x => x.Account.AccountCompany.Id).Distinct().ToList();
			var pIds = cntx_.AccountCompany.Where(x => cIds.Contains(x.Id) && x.ProducerId.HasValue).Select(x => x.ProducerId).Distinct().ToList();
			var plDictionary = cntx_.producernames
				.Where(x => pIds.Contains(x.ProducerId))
				.OrderBy(x => x.ProducerName)
				.ToDictionary(x => x.ProducerId, x => x.ProducerName);
			return plDictionary;
		}

		// возвращает список производителей, от пользователей которых есть сообщения в обратной связи
		private List<SelectListItem> GetProducerListUi(Dictionary<long, string> prDictionary, long? producerId = null)
		{
			var producerList = new List<SelectListItem>() { new SelectListItem { Text = "Выберите производителя", Value = "" } };
			producerList.AddRange(prDictionary.Select(x => new SelectListItem { Text = x.Value, Value = x.Key.ToString(), Selected = producerId == x.Key }));
			return producerList;
		}

		// возвращает минимальную дату получения сообщений обратной связи
		private DateTime? GetMinDate()
		{
			if (cntx_.AccountFeedBack.Any())
				return cntx_.AccountFeedBack.Min(x => x.DateAdd);
			return null;
		}

		// возвращает максимальную дату получения сообщений обратной связи
		private DateTime? GetMaxDate()
		{
			if (cntx_.AccountFeedBack.Any())
				return cntx_.AccountFeedBack.Max(x => x.DateAdd);
			return null;
		}

		private List<SelectListItem> GetItemsPerPageList(int itemsPerPage)
		{
			return new List<SelectListItem>()
				{
						new SelectListItem { Value = "20", Text = "20", Selected = itemsPerPage == 20},
						new SelectListItem { Value = "50", Text = "50", Selected = itemsPerPage == 50 },
						new SelectListItem { Value = "100", Text = "100", Selected = itemsPerPage == 100 },
						new SelectListItem { Value = "1", Text = "1", Selected = itemsPerPage == 50 }
				};
		}

		// возвращает список пользователей, от которых есть сообщения в обратной связи
		private List<SelectListItem> GetAccountList(long? accountId = null)
		{
			var accIds = cntx_.AccountFeedBack.Select(x => x.AccountId).Distinct().ToList();
			var acc = cntx_.Account
				.Where(x => accIds.Contains(x.Id))
				.Select(x => new SelectListItem { Text = x.Login + " " + x.Name, Value = x.Id.ToString(), Selected = accountId == x.Id })
				.OrderBy(x => x.Text).ToList();

			var accountList = new List<SelectListItem>() { new SelectListItem { Value = "", Text = "Выберите пользователя" } };
			accountList.AddRange(acc);
			return accountList;
		}


	}
}