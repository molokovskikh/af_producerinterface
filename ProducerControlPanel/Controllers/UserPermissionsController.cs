using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	///     Редактирование прав пользователя
	/// </summary>
	[AnalitSecuredController]
	[Description("Права пользователей"), MainMenu]
	public class UserPermissionsController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Права";
		}

		/// <summary>
		///     Список прав
		/// </summary>
		/// <returns></returns>
		[Description("Права"), MainMenu]
		public ActionResult ListPermissions()
		{
			var pager = new ModelFilter<UserPermission>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Просмотр прав
		/// </summary>
		public ActionResult InfoPermission(int id)
		{
			var profilePermission = DbSession.Query<UserPermission>().FirstOrDefault(s => s.Id == id);
			if (profilePermission == null) {
				return RedirectToAction("ListPermissions", "UserPermissions");
			}
			ViewBag.CurrentPermission = profilePermission;
			return View();
		}

		/// <summary>
		///     Создание прав
		/// </summary>
		public ActionResult CreatePermission()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			return View();
		}

		[HttpPost]
		public ActionResult CreatePermission([EntityBinder] UserPermission permission)
		{
			var errors = ValidationRunner.Validate(permission);
			if (errors.Count == 0) {
				// сохраняем права 
				DbSession.Save(permission);
				var message = "Новость добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPermissions", "UserPermissions");
			}
			ViewBag.CurrentPermission = permission;
			return View();
		}

		/// <summary>
		///     Редактирование прав
		/// </summary>
		public ActionResult EditPermission(int id)
		{
			var Permission = DbSession.Query<UserPermission>().FirstOrDefault(s => s.Id == id);
			if (Permission == null) {
				return RedirectToAction("ListPermissions", "UserPermissions");
			}
			ViewBag.CurrentPermission = Permission;
			return View();
		}

		[HttpPost]
		public ActionResult EditPermission([EntityBinder] UserPermission permission)
		{
			var errors = ValidationRunner.Validate(permission);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(permission);
				var message = "Новость изменена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPermissions", "UserPermissions");
			}
			ViewBag.CurrentPermission = permission;
			return View();
		}

		/// <summary>
		///     Удаление прав
		/// </summary>
		public ActionResult DeletePermission(int id)
		{
			var currentPermission = DbSession.Query<UserPermission>().FirstOrDefault(s => s.Id == id);
			if (DbSession.AttemptDelete(currentPermission)) {
				var message = "Права удалены успешно";
				SuccessMessage(message);
			}
			else {
				var message = "Права не могут быть удалены";
				ErrorMessage(message);
			}
			return RedirectToAction("ListPermissions", "UserPermissions");
		}

		/// <summary>
		///     Список пользователей
		/// </summary>
		/// <returns></returns>
		[Description("Список пользователей"), MainMenu]
		public ActionResult ListUser()
		{
			var pager = new ModelFilter<ProducerUser>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Управление правами пользователя
		/// </summary>
		/// <returns></returns>
		public ActionResult ManageUserPermission(int id)
		{
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault(s => s.Id == id);
			if (currentUser == null) {
				return RedirectToAction("ListUser", "UserPermissions");
			}
			ViewBag.PermissionsList = DbSession.Query<UserPermission>().Where(s => s != null).ToList()
				.Where(s => !currentUser.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = new UserPermission();
			ViewBag.CurrentUser = currentUser;
			return View();
		}

		/// <summary>
		///     Добавление прав пользователю
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddUserPermission([EntityBinder] ProducerUser currentUser,
			[EntityBinder] UserPermission permission)
		{
			if (currentUser == null || currentUser.Id == 0 || permission == null || permission.Id == 0) {
				return RedirectToAction("ListUser");
			}
			currentUser.Permissions.Add(permission);
			var errors = ValidationRunner.Validate(currentUser);
			if (errors.Count == 0) {
				// сохраняем админа с новыми правами
				DbSession.Save(currentUser);
				var message = "Права добавлены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageUserPermission", "UserPermissions", new {currentUser.Id});
			}
			ViewBag.PermissionsList = DbSession.Query<UserPermission>().Where(s => s != null).ToList()
				.Where(s => !currentUser.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.CurrentUser = currentUser;
			return View("ManageUserPermission", new {currentUser.Id});
		}

		/// <summary>
		///     Удаление прав пользователя
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DeleteUserPermission([EntityBinder] ProducerUser currentUser,
			[EntityBinder] UserPermission permission)
		{
			if (currentUser == null || currentUser.Id == 0 || permission == null || permission.Id == 0) {
				return RedirectToAction("ListUser");
			}
			currentUser.Permissions.Remove(permission);
			var errors = ValidationRunner.Validate(currentUser);
			if (errors.Count == 0) {
				// сохраняем админа с новыми правами
				DbSession.Save(currentUser);
				var message = "Права удалены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageUserPermission", "UserPermissions", new {currentUser.Id});
			}
			ViewBag.PermissionsList = DbSession.Query<UserPermission>().Where(s => s != null).ToList()
				.Where(s => !currentUser.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.CurrentUser = currentUser;
			return View("ManageUserPermission");
		}
	}
}