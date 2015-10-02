using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	/// Страница управления аутентификацией
	/// </summary>
	public class AdminAccountController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Главная";
		}

		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(string username, string password, string returnUrl, bool shouldRemember = false,
			string impersonateClient = "")
		{
			var admin = DbSession.Query<Admin>().FirstOrDefault(p => p.UserName == username);
#if DEBUG
			//Авторизация для тестов, если пароль совпадает с паролем по умолчанию и логин есть в АД, то все ок
			var defaultPassword = Config.GetParam("DefaultUserPassword");
			if (admin != null && password == defaultPassword) {
				Session.Add("employee", admin.Id);
				return Authenticate(username, shouldRemember, admin.Id.ToString());
			}
#endif
			if (ActiveDirectoryHelper.IsAuthenticated(username, password) && admin != null) {
				Session.Add("employee", admin.Id);
				return Authenticate(username, shouldRemember, admin.Id.ToString());
			}
			ErrorMessage("Неправильный логин или пароль");
			return Redirect(returnUrl);
		}

		public ActionResult AdminLogout()
		{
			//FormsAuthentication.SignOut();
			//SetCookie(FormsAuthentication.FormsCookieName, null);
			LogoutUser();
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult ApplyImpersonation([EntityBinder] Admin admin)
		{
			return Authenticate(Environment.UserName, false, admin.Id.ToString());
		}
	}
}