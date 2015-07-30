using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerControlPanel.Models;

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
			if (Request.IsAuthenticated)
				return RedirectToAction("Index", "Admin");
			return View();
		}

		[HttpPost]
		public ActionResult Login(string username, string password, string returnUrl, bool shouldRemember = false, string impersonateClient = "")
		{
			var employee = DbSession.Query<Admin>().FirstOrDefault(p => p.UserName == username);
#if DEBUG
			//Авторизация для тестов, если пароль совпадает с паролем по умолчанию и логин есть в АД, то все ок
			var defaultPassword = ConfigurationManager.AppSettings["DefaultEmployeePassword"];
			if (employee != null && password == defaultPassword) {
				Session.Add("employee", employee.RowID);
				return Authenticate("Index", "Admin", username, shouldRemember, impersonateClient);
			}
#endif
			if (ActiveDirectoryHelper.IsAuthenticated(username, password) && employee != null) {
				Session.Add("employee", employee.RowID);
				return Authenticate("Index", "Admin", username, shouldRemember, impersonateClient);
			}
			ErrorMessage("Неправильный логин или пароль");
			return Redirect(returnUrl);
		}

		public ActionResult AdminLogout()
		{
			FormsAuthentication.SignOut();
			SetCookie(FormsAuthentication.FormsCookieName, null);
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public ActionResult ApplyImpersonation([EntityBinder] Admin admin)
		{
			return Authenticate("Statistic", "AdminAccount", Environment.UserName, false, admin.RowID.ToString());
		}
	}
}