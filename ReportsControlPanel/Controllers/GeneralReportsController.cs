using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using AnalitFramefork.Mvc.Attributes;
using Microsoft.Win32.TaskScheduler;
using NHibernate.Linq;
using ReportsControlPanel.Components;

namespace ReportsControlPanel.Controllers
{
	[Description("Отчеты"), MainMenu(Icon = "entypo-list")]
	public class GeneralReportsController : BaseController
	{

		[Description("Список отчетов"), MainMenu]
		public ActionResult GeneralReportList()
		{
			var filter = new ModelFilter<GeneralReport>(this);
			filter.SetItemsPerPage(1000);
			ViewBag.Filter = filter;
			return View("GeneralReportList");
		}

		/// <summary>
		/// Страница расписания.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <returns></returns>
		[Description("Расписание")]
		public ActionResult Schedule(uint id)
		{

			var report = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			var form = new ReportExecuteForm();
			form.ImportEmailsFromReport(report);
			form.UserEmail = User.Identity.Name.Replace(@"ANALIT\", string.Empty);
			form.UseEmailList = true;

			ViewBag.ReportExecuteForm = form;
			ViewBag.Report = report;
			return View("Schedule");
		}

		/// <summary>
		/// Отправка формы выполнения отчета
		/// </summary>
		/// <param name="reportExecuteForm">Модель формы</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Schedule(uint id, [EntityBinder] ReportExecuteForm reportExecuteForm)
		{
			var report = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			var errors = ValidationRunner.Validate(reportExecuteForm);
			if (errors.Length == 0)
			{
				var useremail = User.Identity.Name.Replace(@"ANALIT\", string.Empty);;
				reportExecuteForm.ProcessReport(report, DbSession, useremail);
				var actionErrors = reportExecuteForm.GetErrors();
				if (actionErrors.Length == 0)
					SuccessMessage(reportExecuteForm.Execute == true ? "Отчет запущен" : "Отчет выслан");
				else
					ErrorMessage(actionErrors[0].Message);
			}
			ViewBag.Report = report;
			ViewBag.ReportExecuteForm = reportExecuteForm;
            return View("Schedule");
		}

		/// <summary>
		/// Страница создания отчета.
		/// </summary>
		/// <returns></returns>
		public ActionResult CreateGeneralReport()
		{
			return SimpleCreate<GeneralReport>();
		}

		/// <summary>
		/// Создание отчета. Обработка действия пользователя.
		/// </summary>
		/// <param name="generalReport">Модель отчета</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CreateGeneralReport(GeneralReport generalReport)
		{
			return SimplePostCreate<GeneralReport>();
		}

		/// <summary>
		/// Страница редактирование отчета. Отображение инофрмации.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <returns></returns>
		public ActionResult EditGeneralReport(int id)
		{
			return SimpleEdit<GeneralReport>(id);
		}

		/// <summary>
		/// Редактирование отчета. Обработка действия пользователя.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="generalReport">Модель отчета</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditGeneralReport(int id, GeneralReport generalReport)
		{
			return SimplePostEdit<GeneralReport>(id);
		}

		/// <summary>
		/// Удаление отчета
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <returns></returns>
		public ActionResult DeleteGeneralReport(uint id)
		{
			SimpleDelete<GeneralReport>((int) id);
			return RedirectToAction("GeneralReportList");
		}

		/// <summary>
		/// Возвращает список значений для плательщиков. Должен вызываться асинхронно
		/// </summary>
		/// <param name="id">Имя плательшика</param>
		/// <returns></returns>
		public JsonResult FindPayersByName(string id = "")
		{
			var payers = DbSession.Query<Payer>().Where(i => i.Name.Contains(id));

			var result = payers.Select(i => new {i.Name, Value = i.Id});
			return Json(result);
		}

		/// <summary>
		/// Изменение плательщика для отчета. Должен вызываться Асинхронно.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="payerId">Идентификатор плательщика</param>
		/// <returns></returns>
		public JsonResult ChangePayer(uint id, uint payerId)
		{
			var report = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			var payer = DbSession.Query<Payer>().First(i => i.Id == payerId);
			report.Payer = payer;
			var errors = ValidationRunner.Validate(report);
			var status = 0;
			if (errors.Length == 0)
			{
				DbSession.Save(report);
				status = 1;
			}

			return Json(new {status});
		}

		/// <summary>
		/// Создание недельного расписания для отчета
		/// </summary>
		/// <param name="id">Идентификатор отчета для которого будет создаваться расписание</param>
		/// <returns></returns>
		public ActionResult CreateWeeklySchedule(uint id)
		{
			var generalReport = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			ViewBag.GeneralReport = generalReport;
			return SimpleCreate<WeeklySchedule>();
		}

		/// <summary>
		/// Сооздание недельного расписания. Обработка действия пользователя.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="weeklySchedule">Модель расписания</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CreateWeeklySchedule(uint id, WeeklySchedule weeklySchedule)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimplePostCreate<WeeklySchedule>(new SimpleData() {SuccessUrl = url});
		}

		/// <summary>
		/// Редактирование недельного расписания отчета.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleid">Идентификатор расписания</param>
		/// <returns></returns>
		public ActionResult EditWeeklySchedule(uint id, int scheduleid)
		{
			return SimpleEdit<WeeklySchedule>(scheduleid);
		}

		/// <summary>
		/// Редактирование недельного расписания отчета. Обработка действия пользователя.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleid">Идентификатор расписания</param>
		/// <param name="weeklySchedule">Модель расписания</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditWeeklySchedule(uint id, int scheduleid, WeeklySchedule weeklySchedule)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimplePostEdit<WeeklySchedule>(weeklySchedule.Id, new SimpleData() {SuccessUrl = url});
		}

		/// <summary>
		/// Удаление недельного расписания отчета
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleId">Идентификатор расписания</param>
		/// <returns></returns>
		public ActionResult DeleteWeeklySchedule(uint id, int scheduleId)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimpleDelete<WeeklySchedule>(scheduleId, new SimpleData() {SuccessUrl = url, FailUrl = url});
		}

		/// <summary>
		/// Создание месячного расписания для отчета
		/// </summary>
		/// <param name="id">Идентификатор отчета для которого будет создаваться расписание</param>
		/// <returns></returns>
		public ActionResult CreateMonthlySchedule(uint id)
		{
			var generalReport = DbSession.Query<GeneralReport>().First(i => i.Id == id);
			ViewBag.GeneralReport = generalReport;
			return SimpleCreate<MonthlySchedule>();
		}

		/// <summary>
		/// Сооздание месячного расписания. Обработка действия пользователя.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="weeklySchedule">Модель расписания</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CreateMonthlySchedule(uint id, MonthlySchedule weeklySchedule)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimplePostCreate<MonthlySchedule>(new SimpleData() {SuccessUrl = url});
		}

		/// <summary>
		/// Редактирование месячного расписания отчета.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleid">Идентификатор расписания</param>
		/// <returns></returns>
		public ActionResult EditMonthlySchedule(uint id, int scheduleid)
		{
			return SimpleEdit<MonthlySchedule>(scheduleid);
		}

		/// <summary>
		/// Редактирование месячного расписания отчета. Обработка действия пользователя.
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleid">Идентификатор расписания</param>
		/// <param name="weeklySchedule">Модель расписания</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditMonthlySchedule(int id, int scheduleid, MonthlySchedule weeklySchedule)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimplePostEdit<MonthlySchedule>(weeklySchedule.Id, new SimpleData() {SuccessUrl = url});
		}

		/// <summary>
		/// Удаление месячного расписания отчета
		/// </summary>
		/// <param name="id">Идентификатор отчета</param>
		/// <param name="scheduleId">Идентификатор расписания</param>
		/// <returns></returns>
		public ActionResult DeleteMonthlySchedule(uint id, int scheduleId)
		{
			var url = Url.Action("Schedule", new {id = id});
			return SimpleDelete<MonthlySchedule>(scheduleId, new SimpleData() {SuccessUrl = url, FailUrl = url});
		}
}
}
