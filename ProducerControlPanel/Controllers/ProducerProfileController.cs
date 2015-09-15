using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Mvc;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	///     Страница профиля пользователя
	/// </summary>
	[Description("Профиль пользователя"), MainMenu]
	public class ProducerProfileController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Профиль пользователя";
		}

		/// <summary>
		///     Список новостей в личном кабинете поставщика
		/// </summary>
		/// <returns></returns>
		[Description("Новости"), MainMenu]
		public ActionResult ListNews()
		{
			var pager = new ModelFilter<ProfileNews>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Просмотр новости из личного кабинета поставщика
		/// </summary>
		public ActionResult InfoNews(int id)
		{
			var profileNews = DbSession.Query<ProfileNews>().FirstOrDefault(s => s.Id == id);
			if (profileNews == null) {
				return RedirectToAction("ListNews");
			}
			ViewBag.CurrentProfileNews = profileNews;
			return View();
		}

		/// <summary>
		///     Создание новости в личном кабинете поставщика
		/// </summary>
		public ActionResult CreateNews()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			return View();
		}

		[HttpPost]
		public ActionResult CreateNews([EntityBinder] ProfileNews profileNews)
		{
			profileNews.CreatedDate = SystemTime.Now();
			profileNews.EditedDate = SystemTime.Now();

			var errors = ValidationRunner.Validate(profileNews);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(profileNews);
				var message = "Новость добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListNews");
			}
			var producerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			ViewBag.CurrentProfileNews = profileNews;
			ViewBag.ProducerList = producerList;
			return View();
		}

		/// <summary>
		///     Редактирование новости в личном кабинете поставщика
		/// </summary>
		public ActionResult EditNews(int id)
		{
			var profileNews = DbSession.Query<ProfileNews>().FirstOrDefault(s => s.Id == id);
			if (profileNews == null) {
				return RedirectToAction("ListNews");
			}
			ViewBag.CurrentProfileNews = profileNews;
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			return View();
		}

		[HttpPost]
		public ActionResult EditNews([EntityBinder] ProfileNews profileNews)
		{
			var errors = ValidationRunner.Validate(profileNews);
			if (errors.Count == 0) {
				profileNews.EditedDate = SystemTime.Now();
				// сохраняем модель нового пользователя 
				DbSession.Save(profileNews);
				var message = "Новость изменена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListNews");
			}
			var producerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			ViewBag.CurrentProfileNews = profileNews;
			ViewBag.ProducerList = producerList;
			return View();
		}

		/// <summary>
		///     Удаление новости в личном кабинете поставщика
		/// </summary>
		public ActionResult DeleteNews(int id)
		{
			var profileNews = DbSession.Query<ProfileNews>().FirstOrDefault(s => s.Id == id);
			if (DbSession.AttemptDelete(profileNews)) {
				var message = "Новость удалена успешно";
				SuccessMessage(message);
			}
			else {
				var message = "Новость не может быть удалена";
				ErrorMessage(message);
			}
			return RedirectToAction("ListNews");
		}

		/// <summary>
		///     Список акций в личном кабинете поставщика
		/// </summary>
		/// <returns></returns>
		[Description("Акции"), MainMenu]
		public ActionResult ListPromotions()
		{
			var pager = new ModelFilter<Promotion>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Просмотр акции из личного кабинета поставщика
		/// </summary>
		public ActionResult InfoPromotion(int id)
		{
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
			if (promotion == null) {
				return RedirectToAction("ListPromotions");
			}
			ViewBag.CurrentPromotion = promotion;
			return View();
		}

		/// <summary>
		///     Зтатус заявки
		/// </summary>
		public ActionResult SetPromotionStatus(int promotionId, bool status)
		{
			var message = "";
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == promotionId);
			if (promotion != null) {
				if (status) {
					promotion.Apply(DbSession, GetCurrentUser());
					message = "Акция активирована";
				}
				else {
					promotion.Decline(DbSession, GetCurrentUser());
					message = "Акция деактивирована";
				}
				SuccessMessage(message);
			}
			else {
				message = "Акция не существует";
				ErrorMessage(message);
			}
			return RedirectToAction("ListPromotions");
		}

		/// <summary>
		///     Создание акции в личном кабинете поставщика
		/// </summary>
		public ActionResult CreatePromotion()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			//TODO:список лекарств (поиск)
			ViewBag.DrugList = DbSession.Query<Drug>().OrderBy(s => s.Name).Take(30).ToList();
			ViewBag.CurrentPromotion = new Promotion();
			return View();
		}

		[HttpPost]
		public ActionResult CreatePromotion([EntityBinder] Promotion promotion)
		{
			promotion.UpdateTime = SystemTime.Now();
			promotion.RegionMask = 1;
			promotion.Admin = GetCurrentUser();
			var errors = ValidationRunner.Validate(promotion);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(promotion);
				var message = "Акция добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPromotions");
			}
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			ViewBag.DrugList = DbSession.Query<Drug>().OrderBy(s => s.Name).Take(30).ToList();
			ViewBag.CurrentPromotion = promotion;
			return View();
		}

		/// <summary>
		///     Редактирование акции в личном кабинете поставщика
		/// </summary>
		public ActionResult EditPromotion(int id)
		{
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
			if (promotion == null) {
				return RedirectToAction("ListPromotions");
			}
			ViewBag.DrugList = DbSession.Query<Drug>().OrderBy(s => s.Name).Take(30).ToList();
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			ViewBag.CurrentPromotion = promotion;
			return View();
		}

		[HttpPost]
		public ActionResult EditPromotion([EntityBinder] Promotion promotion)
		{
			var errors = ValidationRunner.Validate(promotion);
			promotion.Admin = GetCurrentUser();
			if (errors.Count == 0) {
				promotion.UpdateTime = SystemTime.Now();
				// сохраняем модель акции
				DbSession.Save(promotion);
				var message = "Акция изменена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPromotions");
			}
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			ViewBag.DrugList = DbSession.Query<Drug>().OrderBy(s => s.Name).Take(30).ToList();
			ViewBag.CurrentPromotion = promotion;
			return View();
		}

		/// <summary>
		///     Удаление акции в личном кабинете поставщика
		/// </summary>
		public ActionResult DeletePromotion(int id)
		{
			var promotion = DbSession.Query<Promotion>().FirstOrDefault(s => s.Id == id);
			if (DbSession.AttemptDelete(promotion)) {
				var message = "Акция удалена успешно";
				SuccessMessage(message);
			}
			else {
				var message = "Акция не может быть удалена";
				ErrorMessage(message);
			}
			return RedirectToAction("ListPromotions");
		}
	}
}