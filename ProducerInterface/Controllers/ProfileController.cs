using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Mvc;
using AnalitFramefork.Hibernate.Models;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	/// <summary>
	///     Страница профиля пользователя
	/// </summary> 
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
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Name == User.Identity.Name);
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
				regions.Values.Add(new ReportPropertyListValue() { Property = RegionEqual, Value = "1" });
				DbSession.Save(regions);

				var drugs = subreport.Properties.First(i => i.Property.Name == "FullNameEqual");
				var druglist = GetCurrentUser().Producer.Drugs;
				foreach(var drug in druglist)
					drugs.Values.Add(new ReportPropertyListValue() { Property = drugs, Value = drug.Id.ToString() });
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
	}
}