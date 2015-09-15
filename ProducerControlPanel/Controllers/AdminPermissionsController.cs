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
	///     Редактирование прав администраторов
	/// </summary>
	[Description("Права администраторов"), MainMenu]
	public class AdminPermissionsController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Права";
		}

		/// <summary>
		///     Список прав администраторов
		/// </summary>
		/// <returns></returns>
		[Description("Права"), MainMenu]
		public ActionResult ListPermissions()
		{
			var pager = new ModelFilter<AdminPermission>(this);
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
			var profilePermission = DbSession.Query<AdminPermission>().FirstOrDefault(s => s.Id == id);
			if (profilePermission == null) {
				return RedirectToAction("ListPermissions", "AdminPermissions");
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
		public ActionResult CreatePermission([EntityBinder] AdminPermission permission)
		{
			var errors = ValidationRunner.Validate(permission);
			if (errors.Count == 0) {
				// сохраняем права 
				DbSession.Save(permission);
				var message = "Новость добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPermissions", "AdminPermissions");
			}
			ViewBag.CurrentPermission = permission;
			return View();
		}

		/// <summary>
		///     Редактирование прав
		/// </summary>
		public ActionResult EditPermission(int id)
		{
			var Permission = DbSession.Query<AdminPermission>().FirstOrDefault(s => s.Id == id);
			if (Permission == null) {
				return RedirectToAction("ListPermissions", "AdminPermissions");
			}
			ViewBag.CurrentPermission = Permission;
			return View();
		}

		[HttpPost]
		public ActionResult EditPermission([EntityBinder] AdminPermission permission)
		{
			var errors = ValidationRunner.Validate(permission);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(permission);
				var message = "Новость изменена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListPermissions", "AdminPermissions");
			}
			ViewBag.CurrentPermission = permission;
			return View();
		}

		/// <summary>
		///     Удаление прав
		/// </summary>
		public ActionResult DeletePermission(int id)
		{
			var currentPermission = DbSession.Query<AdminPermission>().FirstOrDefault(s => s.Id == id);
			if (DbSession.AttemptDelete(currentPermission)) {
				var message = "Права удалены успешно";
				SuccessMessage(message);
			}
			else {
				var message = "Права не могут быть удалены";
				ErrorMessage(message);
			}
			return RedirectToAction("ListPermissions", "AdminPermissions");
		}

		/// <summary>
		///     Список администраторов
		/// </summary>
		/// <returns></returns>
		[Description("Список администраторов"), MainMenu]
		public ActionResult ListAdmin()
		{
			var pager = new ModelFilter<Admin>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Управление правами администратора
		/// </summary>
		/// <returns></returns>
		public ActionResult ManageAdminPermission(int id)
		{
			var currentAdmin = DbSession.Query<Admin>().FirstOrDefault(s => s.Id == id);
			if (currentAdmin == null) {
				return RedirectToAction("ListAdmin", "AdminPermissions");
			}
			ViewBag.PermissionsList = DbSession.Query<AdminPermission>().Where(s => s != null).ToList()
				.Where(s => !currentAdmin.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = new AdminPermission();
			ViewBag.CurrentAdmin = currentAdmin;
			return View();
		}

		/// <summary>
		///     Добавление прав администратора
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddAdminPermission([EntityBinder] Admin currentAdmin, [EntityBinder] AdminPermission permission)
		{
			if (currentAdmin == null || currentAdmin.Id == 0 || permission == null || permission.Id == null) {
				return RedirectToAction("ListAdmin", "AdminPermissions");
			}
			currentAdmin.Permissions.Add(permission);
			var errors = ValidationRunner.Validate(currentAdmin);
			if (errors.Count == 0) {
				// сохраняем админа с новыми правами
				DbSession.Save(currentAdmin);
				var message = "Права добавлены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageAdminPermission", "AdminPermissions", new { currentAdmin.Id });
			}
			ViewBag.PermissionsList = DbSession.Query<AdminPermission>().Where(s => s != null).ToList()
				.Where(s => !currentAdmin.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.CurrentAdmin = currentAdmin;
			return View("ManageAdminPermission", new {currentAdmin.Id});
		}

		/// <summary>
		///     Удаление прав администратора
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DeleteAdminPermission([EntityBinder] Admin currentAdmin, [EntityBinder] AdminPermission permission)
		{
			if (currentAdmin == null || currentAdmin.Id == 0 || permission == null || permission.Id == null)
			{
				return RedirectToAction("ListAdmin", "AdminPermissions");
			}
			currentAdmin.Permissions.Remove(permission);
			var errors = ValidationRunner.Validate(currentAdmin);
			if (errors.Count == 0) {
				// сохраняем админа с новыми правами
				DbSession.Save(currentAdmin);
				var message = "Права удалены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageAdminPermission", "AdminPermissions", new { currentAdmin.Id });
			}
			ViewBag.PermissionsList = DbSession.Query<AdminPermission>().Where(s => s != null).ToList()
				.Where(s => !currentAdmin.Permissions.Contains(s)).OrderBy(s => s.Description).ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.CurrentAdmin = currentAdmin;
			return View("ManageAdminPermission");
		}
	}
}