using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	public class BaseProducerInterfaceController : BaseController
	{
		//
		// GET: /BaseInterfaceController/

		[Description("Текущий пользователь")]
		private ProducerUser CurrentProducerUser { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// думаю, это нужно кинуть в Tuple<> список или просто по две переменные: Controll, Action, чтобы дальше использовать в Url.Action() и GetPermissionName()
			const string unRegisteredRedirect = "registration_index";
			const string unAuthorizedRedirect = "registration_userauthentication";
			const string notConfirmedRedirect = "registration_gegistrationconfirm";
			const string noPermissionRedirect = "home_Index";

			base.OnActionExecuting(filterContext);
			var user = GetCurrentUser();
			ViewBag.CurrentUser = user;

			var actionName = filterContext.RouteData.Values["action"].ToString();
			var controllerName = GetType().Name.Replace("Controller", "");

			//Обновление прав при их отсуствии
			if (DbSession.Query<UserPermission>().Count() == 0) {
				UserPermission.UpdatePermissions<UserPermission>(DbSession, this, typeof (BaseProducerInterfaceController));
				//удаление ненужных прав
				var ListToRemove = DbSession.Query<UserPermission>().ToList()
					.Where(s => s.Name.ToLower().IndexOf("home_") != -1
					            || s.Name.ToLower().IndexOf("registration_") != -1
					).ToList();
				ListToRemove.ForEach(s => { DbSession.Delete(s); });
			}


			string currentPermissionName = GetPermissionName(controllerName, actionName);
			var currentPermission = GetActionPermission(currentPermissionName);

			//редирект для неавторизованного пользователя
			if ((user == null && currentPermission != null)
			    && (currentPermissionName != unRegisteredRedirect
			        && currentPermissionName != unAuthorizedRedirect
			        && currentPermissionName != notConfirmedRedirect)) {
				ErrorMessage("Для входа перехода на данную страницу Вам необходимо зарегистрироваться.");
				RedirectUnAuthorizedUser(filterContext);
				return;
			}
			//редирект для пользователя, без соответствующих прав
			if (user != null && currentPermission != null && !user.CheckPermission(currentPermission)
			    && currentPermissionName != noPermissionRedirect) {
				var loginUrl = Url.Action("Index", "Home");
				ErrorMessage("У Вас нет прав доступа к запрашиваемой странице.");
				RedirectUnAuthorizedUser(filterContext, loginUrl, false);
				return;
			}
		}

		public ProducerUser GetCurrentUser(bool getFromSession = true)
		{
			if (CurrentAnalitUser == null || (CurrentAnalitUser.Name == String.Empty) || DbSession == null || !DbSession.IsConnected) {
				CurrentProducerUser = null;
				return null;
			}
			if (getFromSession && (CurrentProducerUser == null || User.Identity.Name != CurrentProducerUser.Name)) {
				CurrentProducerUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Email == CurrentAnalitUser.Name);
			}
			return CurrentProducerUser;
		}

		/// <summary>
		///     Получение прав для текущего экшена
		/// </summary>
		/// <param name="permissionName"></param>
		/// <returns>Права</returns>
		public UserPermission GetActionPermission(string permissionName)
		{
			var permission =
				DbSession.Query<UserPermission>()
					.FirstOrDefault(s => s.Name.ToLower() == permissionName);
			return permission;
		}
	}
}