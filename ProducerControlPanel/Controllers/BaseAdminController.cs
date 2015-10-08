using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
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
			const string ignorePermissionRedirect = "admin_index";

			base.OnActionExecuting(filterContext);
			var actionName = filterContext.RouteData.Values["action"].ToString();
			var controllerName = GetType().Name.Replace("Controller", "");

			var admin = GetCurrentUser();

			ViewBag.Admin = admin;
			ViewBag.JavascriptParams["baseurl"] = GetBaseUrl();
			ViewBag.ActionName = actionName;
			ViewBag.ControllerName = controllerName;
			ViewBag.Controller = this;
			//Проверка прав
			AuthenticationModule.CheckPermissions(DbSession, filterContext, admin, ignorePermissionRedirect);

			//Если у контроллера, есть описательный аттрибут, то создаем хлебные крошки
			var hasDescription = Attribute.IsDefined(GetType(), typeof (DescriptionAttribute));
			if (hasDescription)
				SetBreadcrumb(this.GetDescription());
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