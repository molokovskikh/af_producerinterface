using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ReportsControlPanel.Models;

namespace ReportsControlPanel.Controllers
{
	[Description("Отчеты")]
	public class GeneralReportsController : BaseController
	{

		public ActionResult GeneralReportList()
		{
			var filter = new ModelFilter<GeneralReport>(this);
			filter.SetItemsPerPage(1000);
			ViewBag.Filter = filter;
			return View("GeneralReportList");
		}

		public ActionResult CreateGeneralReport()
		{
			return SimpleCreate<GeneralReport>();
		}

		[HttpPost]
		public ActionResult CreateGeneralReport(GeneralReport generalReport)
		{
			return SimplePostCreate<GeneralReport>();
		}

		public ActionResult EditGeneralReport(int id)
		{
			return SimpleEdit<GeneralReport>(id);
		}

		[HttpPost]
		public ActionResult EditGeneralReport(int id, GeneralReport generalReport)
		{
			return SimplePostEdit<GeneralReport>(id);
		}

		public ActionResult DeleteGeneralReport(uint id)
		{
			SimpleDelete<GeneralReport>(id);
			return RedirectToAction("GeneralReportList");
		}


		public JsonResult FindPayersByName(string id = "")
		{
			var payers = DbSession.Query<Payer>().Where(i => i.Name.Contains(id));

			var result = payers.Select(i => new { i.Name, Value = i.Id });
			return Json(result);
		}

		public JsonResult ChangePayer(uint id, uint payerId)
		{
			var report = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			var payer = DbSession.Query<Payer>().First(i => i.Id == payerId);
			report.Payer = payer;
			var errors = ValidationRunner.Validate(report);
			var status = 0;
			if (errors.Length == 0) {
				DbSession.Save(report);
				status = 1;
			}

			return Json(new { status });
		}
	}
}
