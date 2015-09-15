using System.Linq;
using System.Web.Mvc;
using Analit.Components;
using AnalitFramefork.Components;
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
	}
}