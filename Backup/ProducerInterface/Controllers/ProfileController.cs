using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	/// <summary>
	/// Страница профиля пользователя
	/// </summary>
	[Authorize]
	public class ProfileController : BaseInterfaceController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Профиль пользователя";
		}

		public ActionResult Index()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			var currentUser = DbSession.Query<User>().FirstOrDefault(e => e.Name == User.Identity.Name);
			ViewBag.CurrentUser = currentUser;
			ViewBag.ProducerUserList = DbSession.Query<User>().Where(e => e.Producer == currentUser.Producer).OrderBy(s=>s.Name).ToList();
			return View();
		}
	}
}