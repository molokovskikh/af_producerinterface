using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Analit.Components;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	///     Страница профиля пользователя
	/// </summary>
	[Authorize]
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
	}
}