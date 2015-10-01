using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;

namespace ProducerControlPanel.Controllers
{
	public class BaseAdminController : BaseController
	{
		//
		// GET: /BaseAdminController/

		[Description("Текущий пользователь")]
		private Admin CurrentUser { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			const string noPermissionRedirect = "admin_index";

			base.OnActionExecuting(filterContext);

			var admin = GetCurrentUser();
			ViewBag.Admin = admin;
			ViewBag.JavascriptParams["baseurl"] = GetBaseUrl();

			var actionName = filterContext.RouteData.Values["action"].ToString();
			var controllerName = GetType().Name.Replace("Controller", "");
			string currentPermissionName = GetPermissionName(controllerName, actionName);

			var currentPermission = GetActionPermission(currentPermissionName);

			ViewBag.ActionName = actionName;
			ViewBag.ControllerName = controllerName;
			ViewBag.Controller = this;
			
			//редирект для пользователя, без соответствующих прав
			if (admin != null && currentPermission != null && !admin.CheckPermission(currentPermission)
			    && currentPermissionName != noPermissionRedirect) {
				var redirectUrl = Url.Action("Index", "Admin"); // Default Login Url 
				ErrorMessage("У Вас нет прав доступа к запрашиваемой странице.");
				RedirectUnAuthorizedUser(filterContext, redirectUrl, false);
				return;
			}

			//Если у контроллера, есть описательный аттрибут, то создаем хлебные крошки
			var hasDescription = Attribute.IsDefined(GetType(), typeof (DescriptionAttribute));
			if (hasDescription)
				SetBreadcrumb(this.GetDescription());
		}

		/// <summary>
		///     Получение прав для текущего экшена
		/// </summary>
		/// <returns>Права</returns>
		public AdminPermission GetActionPermission(string permissionName)
		{
			var permission =
				DbSession.Query<AdminPermission>()
					.FirstOrDefault(s => s.Name.ToLower() == permissionName);
			return permission;
		}

		/// <summary>
		/// Получение текущего пользователя
		/// </summary>
		/// <returns></returns>
		public Admin GetCurrentUser(bool getFromSession = true)
		{
			if (CurrentAnalitUser == null || (CurrentAnalitUser.Name == string.Empty) || DbSession == null ||
			    !DbSession.IsConnected) {
				CurrentUser = null;
				return null;
			}
			if (getFromSession && (CurrentUser == null || CurrentAnalitUser.Name != CurrentUser.UserName)) {
				CurrentUser = DbSession.Query<Admin>().FirstOrDefault(e => e.UserName == CurrentAnalitUser.Name);
			}
			return CurrentUser;
		}
	}
}