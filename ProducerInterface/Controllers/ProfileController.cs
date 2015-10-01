using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Mvc;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	/// <summary>
	///     Страница профиля пользователя
	/// </summary> 
	[AnalitSecuredController]
	public class ProfileController : BaseProducerInterfaceController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Профиль пользователя";
		}

		public ActionResult Index()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			var currentUser = GetCurrentUser();
			ViewBag.CurrentUser = currentUser;
			ViewBag.ProducerUserList =
				DbSession.Query<ProducerUser>().Where(e => e.Producer == currentUser.Producer).OrderBy(s => s.Name).ToList();
			ViewBag.ProfileNewsList =
				DbSession.Query<ProfileNews>()
					.Where(e => e.Producer == currentUser.Producer)
					.OrderByDescending(s => s.CreatedDate)
					.ThenBy(s => s.Topic)
					.ToList();
			return View("Index");
		}

		public ActionResult Catalog(int? id = null)
		{
			if (id.HasValue) {
				var family = DbSession.Query<DrugFamily>().FirstOrDefault(i => i.Id == id.Value);
				ViewBag.DrugFamily = family;
			}
			return View("Catalog");
		}

		public ActionResult CreateDrugDescriptionRemark(int id)
		{
			var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
			var remark = new DrugDescriptionRemark(family);
			var mnns = DbSession.Query<MNN>().ToList();
			ViewBag.AvailibleMnn = mnns;
			ViewBag.DrugDescriptionRemark = remark;
			return View("CreateDrugDescriptionRemark");
		}

		[HttpPost]
		public ActionResult CreateDrugDescriptionRemark(int id, [EntityBinder] DrugDescriptionRemark drugDescriptionRemark)
		{
			var user = GetCurrentUser();
			var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
			drugDescriptionRemark.ProducerUser = user;
			drugDescriptionRemark.DrugFamily = family;
			var errors = ValidationRunner.Validate(drugDescriptionRemark);
			if (errors.Length == 0) {
				DbSession.Save(drugDescriptionRemark);
				SuccessMessage("Запрос на изменение описания отправлен модератору.");
				return Redirect(GetIndexActionUrl());
			}
			ErrorMessage("Произошла ошибка.");
			CreateDrugDescriptionRemark(id);
			ViewBag.DrugDescriptionRemark = drugDescriptionRemark;
			return View("CreateDrugDescriptionRemark");
		}

		public ActionResult Promotions()
		{
			var currentUser = GetCurrentUser();
			var list = DbSession.Query<Promotion>().Where(s => s.Producer == currentUser.Producer && s.Status).ToList();
			ViewBag.PromotionList = list;
			return View();
		}

		/// <summary>
		///     Создание акции в личном кабинете поставщика
		/// </summary>
		public ActionResult ManagePromotion(int? id)
		{
			var currentUser = GetCurrentUser();
			if (id.HasValue) {
				ViewBag.CurrentPromotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
			}
			else {
				ViewBag.CurrentPromotion = new Promotion();
			}

			return View();
		}

		[HttpPost]
		public ActionResult ManagePromotion([EntityBinder] Promotion promotion)
		{
			var user = GetCurrentUser();
			promotion.UpdateTime = SystemTime.Now();
			promotion.RegionMask = 1;
			promotion.Producer = user.Producer;
			promotion.ProducerUser = user;
			promotion.Status = false;
			var errors = ValidationRunner.Validate(promotion);
			if (errors.Length == 0) {
				DbSession.Save(promotion);
				SuccessMessage("Запрос на подтверждение акции отправлен модератору.");
				return RedirectToAction("ManagePromotion", new {promotion.Id});
			}
			ErrorMessage("Произошла ошибка.");
			ViewBag.CurrentPromotion = promotion;
			return View();
		}

		[HttpPost]
		public ActionResult AddPromotionDrug(int promotionId, int drugId)
		{
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == promotionId);
			var drug = DbSession.Query<Drug>().FirstOrDefault(s => s.Id == drugId);
			if (promotion != null && drug != null) {
				promotion.UpdateTime = SystemTime.Now();
				promotion.Status = false;
				if (!promotion.Drugs.Contains(drug)) {
					promotion.Drugs.Add(drug);
				}
				var errors = ValidationRunner.Validate(promotion);
				if (errors.Length == 0) {
					DbSession.Save(promotion);
					SuccessMessage("Запрос на подтверждение акции отправлен модератору.");
				}
				ErrorMessage("Произошла ошибка.");
			}
			else {
				ErrorMessage("Акция отсутствует.");
			}
			return RedirectToAction("ManagePromotion", new { id = promotionId });
		}

		[HttpPost]
		public ActionResult RemovePromotionDrug(int promotionId, int drugId)
		{
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == promotionId);
			var drug = DbSession.Query<Drug>().FirstOrDefault(s => s.Id == drugId);
			if (promotion != null) {
				promotion.UpdateTime = SystemTime.Now();
				promotion.Status = false;
				if (promotion.Drugs.Contains(drug)) {
					promotion.Drugs.Remove(drug);
				}
				var errors = ValidationRunner.Validate(promotion);
				if (errors.Length == 0) {
					DbSession.Save(promotion);
					SuccessMessage("Запрос на подтверждение акции отправлен модератору.");
				}
				ErrorMessage("Произошла ошибка.");
			}
			else {
				ErrorMessage("Акция отсутствует.");
			}
			return RedirectToAction("ManagePromotion", new {id=promotionId});
		}

		public ActionResult Reports()
		{
			var user = GetCurrentUser();
			var templates = user.Producer.GetReportTemplates();
			ViewBag.ReportTemplates = templates;
            return View("Reports");
		}

		public ActionResult CreateRatingReport()
		{
			var reporttype = DbSession.Query<ReportType>().First(i => i.FilePrefix == "Rating");
			var form = new ReportExecuteForm();
			form.UseEmailList = false;
            ViewBag.Form = form;
			ViewBag.ReportType = reporttype;
			return View("CreateRatingReport");
		}

		[HttpPost]
		public ActionResult CreateRatingReport([EntityBinder] ReportExecuteForm reportExecuteForm)
		{
			var report = new GeneralReport();
			report.Title = "Отчет пользователя " + GetCurrentUser().Email + ", поставщика "+ GetCurrentUser().Producer.Name + " (отключен)";
			report.ReportFileName = "Рейтинговый отчет WORWAG PHARMA GmbH";
			report.Enabled = false;
			var subreport = new Report();
			subreport.Type = DbSession.Query<ReportType>().First(i => i.Id == 6);
			subreport.Name = "Отчет пользователя " + GetCurrentUser().Email + ", поставщика " + GetCurrentUser().Producer.Name + " (отключен)";
			subreport.Enabled = true;
			
			//Оказывается, что обязательные параметры создаются триггером
			//var junk = new ReportPropertyValue();
			//junk.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "JunkState");
			//junk.Value = "0";
			//subreport.Properties.Add(junk);

			//var ReportInterval = new ReportPropertyValue();
			//ReportInterval.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "ReportInterval");
			//ReportInterval.Value = "27";
			//subreport.Properties.Add(ReportInterval);

			//var ByPreviousMonth = new ReportPropertyValue();
			//ByPreviousMonth.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "ByPreviousMonth");
			//ByPreviousMonth.Value = "1";
			//subreport.Properties.Add(ByPreviousMonth);

			//var BuildChart = new ReportPropertyValue();
			//BuildChart.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "BuildChart");
			//BuildChart.Value = "0";
			//subreport.Properties.Add(BuildChart);

			//var DoNotShowAbsoluteValues = new ReportPropertyValue();
			//DoNotShowAbsoluteValues.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "DoNotShowAbsoluteValues");
			//DoNotShowAbsoluteValues.Value = "0";
			//subreport.Properties.Add(DoNotShowAbsoluteValues);

			var FullNamePosition = new ReportPropertyValue();
			FullNamePosition.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "FullNamePosition");
			FullNamePosition.Value = "0";
			subreport.Properties.Add(FullNamePosition);

			var FullNameEqual = new ReportPropertyValue();
			FullNameEqual.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "FullNameEqual");
			FullNameEqual.Value = "1";
			subreport.Properties.Add(FullNameEqual);

			var RegionPosition = new ReportPropertyValue();
			RegionPosition.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "RegionPosition");
			RegionPosition.Value = "3";
			subreport.Properties.Add(RegionPosition);

			var FirmCrPosition = new ReportPropertyValue();
			FirmCrPosition.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "FirmCrPosition");
			FirmCrPosition.Value = "2";
			subreport.Properties.Add(FirmCrPosition);

			var FirmCodePosition = new ReportPropertyValue();
			FirmCodePosition.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "FirmCodePosition");
			FirmCodePosition.Value = "4";
			subreport.Properties.Add(FirmCodePosition);

			var RegionEqual = new ReportPropertyValue();
			RegionEqual.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "RegionEqual");
			RegionEqual.Value = "1";
			subreport.Properties.Add(RegionEqual);

			var FirmCodeNonEqual = new ReportPropertyValue();
			FirmCodeNonEqual.Property = DbSession.Query<ReportTypeProperty>().First(i => i.Name == "FirmCodeNonEqual");
			FirmCodeNonEqual.Value = "1";
			subreport.Properties.Add(FirmCodeNonEqual);
			foreach (var prop in subreport.Properties)
				prop.Report = subreport;
			reportExecuteForm.UserEmail = GetCurrentUser().Email;
			var errors = ValidationRunner.Validate(reportExecuteForm);
			if (errors.Length == 0)
			{
				DbSession.Save(report);
				subreport.GeneralReport = report;
				DbSession.Save(subreport);
				DbSession.Flush();
				DbSession.Refresh(subreport);

				var ByPreviousMonth= subreport.Properties.First(i => i.Property.Name == "ByPreviousMonth");
				ByPreviousMonth.Value = "1";
				var ReportInterval = subreport.Properties.First(i => i.Property.Name == "ReportInterval");
				ReportInterval.Value = "27";

				var regions = subreport.Properties.First(i => i.Property.Name == "RegionEqual");
			//	regions.Values.Add(new ReportPropertyListValue() { Property = RegionEqual, Value = "1" });
				DbSession.Save(regions);

				var drugs = subreport.Properties.First(i => i.Property.Name == "FullNameEqual");
				var druglist = GetCurrentUser().Producer.Drugs;
				foreach (var drug in druglist) ;
			//		drugs.Values.Add(new ReportPropertyListValue() { Property = drugs, Value = drug.Id.ToString() });
				DbSession.Save(drugs);

				DbSession.Save(subreport);
				DbSession.Flush();

				reportExecuteForm.ProcessReport(report, DbSession, GetCurrentUser().Email);
				var actionErrors = reportExecuteForm.GetErrors();
				if (actionErrors.Length == 0)
					SuccessMessage(reportExecuteForm.Execute == true ? "Отчет запущен" : "Отчет выслан");
				else
					ErrorMessage(actionErrors[0].Message);
			}

			ViewBag.ReportExecuteForm = reportExecuteForm;
			return View("CreateRatingReport");
		}

		public ActionResult CreateReportTemplate()
		{
			return SimpleCreate<ReportTemplate>();
		}

		[HttpPost]
		public ActionResult CreateReportTemplate([EntityBinder] ReportTemplate reportTemplate)
		{
			reportTemplate.GeneralReport = new GeneralReport();
			reportTemplate.User = GetCurrentUser();
			reportTemplate.Update(reportTemplate.User);
			var errors = ValidationRunner.Validate(reportTemplate);
			if (errors.Length == 0)
			{
				reportTemplate.GeneralReport.Title = "Отчет пользователя " + GetCurrentUser().Email + ", поставщика " + GetCurrentUser().Producer.Name + "";
				reportTemplate.GeneralReport.ReportFileName = reportTemplate.Title;
				reportTemplate.GeneralReport.Enabled = false;
				DbSession.Save(reportTemplate.GeneralReport);
				DbSession.Save(reportTemplate);
				//Создание подотчета 
				//var subreport = new Report();
				//subreport.Name = "Отчет пользователя " + GetCurrentUser().Email + ", поставщика " + GetCurrentUser().Producer.Name + " (отключен)";
				//subreport.Enabled = true;
				//subreport.GeneralReport = reportTemplate.GeneralReport;
				//switch (reportTemplate.Type) 
				//{
				//	case ReportTemplateType.ProducerRating:
				//		subreport.Type = DbSession.Query<ReportType>().First(i => i.Id == 6);
				//		break;
				//}
				//DbSession.Save(subreport);
				SuccessMessage("Отчет успешно создан");
				return RedirectToAction("EditReportTemplate", new {id = reportTemplate.Id});
			}

			ViewBag.ReportTemplate = reportTemplate;
			return View("CreateReportTemplate");
		}

		public ActionResult EditReportTemplate(int id)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var form = ReportTemplateForm.CreateReportTemplateForm(template, DbSession);
            ViewBag.ReportTemplateForm = form;
			return View("EditReportTemplate");
		}

		[HttpPost]
		public ActionResult EditReportTemplate(int id, ReportTemplate reportTemplate)
		{
			//Так как мы не знаем изначально тип формы которая нам высылается, придется создать сначала форму данного типа, а потом уже забиндить ее
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var form = ReportTemplateForm.CreateReportTemplateForm(template, DbSession);
			var binder = new EntityBinder();
			binder.MapModel(Request, form);
			form.Refresh();
			var errors = ValidationRunner.Validate(form);
			if (errors.Length == 0)
			{
				form.Save(DbSession);
				SuccessMessage("Изменения успешно внесены");
				return RedirectToAction("EditReportTemplate", new {id});
			}
			ErrorMessage(errors.First().Message);
            ViewBag.ReportTemplateForm = form;
			return View("EditReportTemplate");
		}

		public ActionResult EditGeneralReport(int id)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			ViewBag.ReportTemplate = template;
			return View("EditGeneralReport");
		}

		public ActionResult RunGeneralReport(int id)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var form = new ReportExecuteForm();
			form.UseEmailList = false;
			ViewBag.Form = form;
			ViewBag.ReportTemplate = template;
			return View("RunGeneralReport");
		}

		[HttpPost]
		public ActionResult RunGeneralReport(int id, [EntityBinder] ReportExecuteForm reportExecuteForm)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			reportExecuteForm.UserEmail = GetCurrentUser().Email;
			var errors = ValidationRunner.Validate(reportExecuteForm);
			if (errors.Length == 0)
			{
				reportExecuteForm.ProcessReport(template.GeneralReport, DbSession, GetCurrentUser().Email);
				var actionErrors = reportExecuteForm.GetErrors();
				if (actionErrors.Length == 0)
					SuccessMessage(reportExecuteForm.Execute == true ? "Отчет запущен" : "Отчет выслан");
				else
					ErrorMessage(actionErrors[0].Message);
			}
			else
				ErrorMessage(errors.First().Message);

			ViewBag.Form = reportExecuteForm;
			ViewBag.ReportTemplate = template;
			return View("RunGeneralReport");
		}

		public ActionResult CreateMonthlySchedule(int id)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var schedule = new MonthlySchedule();
			schedule.GeneralReport = template.GeneralReport;
			ViewBag.MonthlySchedule = schedule;
			return View("CreateMonthlySchedule");
		}

		[HttpPost]
		public ActionResult CreateMonthlySchedule(int id, MonthlySchedule monthlySchedule)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var url = Url.Action("EditGeneralReport", new {id = template.Id});
			return SimplePostEdit<MonthlySchedule>(monthlySchedule.Id, new SimpleData() { SuccessUrl = url });
		}

		public ActionResult CreateWeeklySchedule(int id)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var schedule = new WeeklySchedule();
			schedule.GeneralReport = template.GeneralReport;
			ViewBag.WeeklySchedule = schedule;
			return View("CreateWeeklySchedule");
		}

		[HttpPost]
		public ActionResult CreateWeeklySchedule(int id, WeeklySchedule weeklySchedule)
		{
			var template = DbSession.Query<ReportTemplate>().First(i => i.Id == id);
			var url = Url.Action("EditGeneralReport", new { id = template.Id });
			return SimplePostEdit<WeeklySchedule>(weeklySchedule.Id, new SimpleData() { SuccessUrl = url });
		}

		public JsonResult FindSuppliers(string id, string regions, string suppliers)
		{
			var regionNames = regions.Split(',');
			var regionModels = DbSession.Query<Region>().Where(i => regionNames.Contains(i.Title)).ToList();
			var supplierNames = suppliers.Split(',');
			var supplierModels = DbSession.Query<Supplier>().Where(i => supplierNames.Contains(i.Name)).ToList();
			var foundSuppliers = ProducerRatingReportTemplateForm.FindSuppliers(DbSession, id, regionModels, supplierModels);
			var result = foundSuppliers.Select(i => new {Name = i.Name, Value = i.Id});
			return Json(result);
		}

		public JsonResult FindRegions(string id, string regions)
		{
			var regionNames = regions.Split(',');
			var regionModels = DbSession.Query<Region>().Where(i => regionNames.Contains(i.Title)).ToList();
			var foundRegions = ProducerRatingReportTemplateForm.FindRegions(DbSession, id, regionModels);
			var result = foundRegions.Select(i => new {Name = i.Title, Value = i.Id});
			return Json(result);
		}

		public JsonResult FindDrugs(string id, string drugs)
		{
			var drugNames = drugs.Split(',');
			var drugModels = DbSession.Query<Drug>().Where(i => drugNames.Contains(i.Name)).ToList();
			var producer = GetCurrentUser().Producer;
			var foundDrugs = ProducerRatingReportTemplateForm.FindDrugs(DbSession, id, producer, drugModels);
			var result = foundDrugs.Select(i => new {Name = i.Name, Value = i.Id});
			return Json(result);
		}
	}
}