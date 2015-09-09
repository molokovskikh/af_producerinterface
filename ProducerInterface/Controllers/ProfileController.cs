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
				DbSession.Query<ProfileNews>().Where(e => e.Producer == currentUser.Producer).OrderByDescending(s => s.CreatedDate).ThenBy(s=>s.Topic).ToList();
			return View();
		}

		public ActionResult CreateNews()
		{
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Name == User.Identity.Name);
			ViewBag.CurrentUser = currentUser;
			return View();
		}

		[HttpPost]
		public ActionResult CreateNews([EntityBinder] ProfileNews profileNews)
		{
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Name == User.Identity.Name);
			profileNews.Producer = DbSession.Query<Producer>().FirstOrDefault();
			profileNews.CreatedDate = SystemTime.Now();
			profileNews.EditedDate = SystemTime.Now();
			var errors = ValidationRunner.Validate(profileNews);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(profileNews);
				var message = "Новость добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("Index");
			}

			ViewBag.CurrentProfileNews = profileNews;
			ViewBag.CurrentUser = currentUser;
			return View();
		}
	}
}