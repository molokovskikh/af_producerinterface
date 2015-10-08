using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate;
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
			const string ignorePermissionRedirect = "home_Index";

			base.OnActionExecuting(filterContext);
			var user = GetCurrentUser();
			ViewBag.CurrentUser = user;

			//Проверка наличия прав
			CheckForExistenceOfUserPermissions(DbSession, this);
			//Проверка прав
			AuthenticationModule.CheckPermissions(DbSession, filterContext, user, ignorePermissionRedirect);
		}

		/// <summary>
		/// Получение текущего пользователя
		/// </summary>
		/// <param name="getFromSession">По умолчанию пользователь получается запросом</param>
		/// <returns></returns>
		public ProducerUser GetCurrentUser(bool getFromSession = true)
		{
			if (CurrentAnalitUser == null || (CurrentAnalitUser.Name == String.Empty) || DbSession == null ||
			    !DbSession.IsConnected) {
				CurrentProducerUser = null;
				return null;
			}
			if (getFromSession && (CurrentProducerUser == null || CurrentAnalitUser.Name != CurrentProducerUser.Email)) {
				CurrentProducerUser = DbSession.Query<ProducerUser>().FirstOrDefault(e => e.Email == CurrentAnalitUser.Name);
			}
			return CurrentProducerUser;
		}
		/// <summary>
		/// Проверка наличия прав в БД
		/// </summary>
		/// <param name="dbSession">Хибер-сессия</param>
		/// <param name="controller">Контроллер</param>
		public static void CheckForExistenceOfUserPermissions(ISession dbSession, Controller controller)
		{
			//Обновление прав при их отсуствии
			if (dbSession.Query<UserPermission>().Count() == 0) {
				UserPermission.UpdatePermissions<UserPermission>(dbSession, controller, typeof (BaseProducerInterfaceController));
				//удаление ненужных прав
				var ListToRemove = dbSession.Query<UserPermission>().ToList()
					.Where(s => s.Name.ToLower().IndexOf("home_") != -1
					            || s.Name.ToLower().IndexOf("registration_") != -1
					).ToList();
				ListToRemove.ForEach(s => { dbSession.Delete(s); });
			}
		}
	}
}