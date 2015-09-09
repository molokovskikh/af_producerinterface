using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerInterface.Controllers
{
	/// <summary>
	///     Главная
	/// </summary>
	[Description("Права пользователей"), MainMenu]
	public class PermissionsController : BaseProducerInterfaceController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Права";
		}

		/// <summary>
		///     Список пользователей
		/// </summary>
		/// <returns></returns>
		[Description("Список пользователей"), MainMenu]
		public ActionResult ListUser()
		{
			var currentUser = GetCurrentUser();
			var pager = new ModelFilter<ProducerUser>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria(s => s.Producer == currentUser.Producer && s.Id != currentUser.Id);
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Управление прав
		/// </summary>
		/// <returns></returns>
		public ActionResult ManageUserPermission(int id)
		{
			var ModifiedUser = DbSession.Query<ProducerUser>().FirstOrDefault(s => s.Id == id);
			if (ModifiedUser == null || ModifiedUser.Producer != GetCurrentUser().Producer) {
				return RedirectToAction("ListUser", "UserPermissions");
			}
			ViewBag.PermissionsList =
				DbSession.Query<UserPermission>().Where(s => s != null).ToList()
					.Where(s => !ModifiedUser.Permissions.Contains(s))
					.OrderBy(s => s.Description)
					.ToList();
			ViewBag.CurrentPermissions = new UserPermission();
			ViewBag.ModifiedUser = ModifiedUser;
			return View();
		}

		/// <summary>
		///     Добавление прав
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddUserPermission([EntityBinder] ProducerUser ModifiedUser,
			[EntityBinder] UserPermission permission)
		{
			if (ModifiedUser == null || ModifiedUser.Id == 0 || permission == null || permission.Id == 0) {
				return RedirectToAction("ListUser");
			}
			ModifiedUser.Permissions.Add(permission);
			var errors = ValidationRunner.Validate(ModifiedUser);
			if (errors.Count == 0) {
				// сохраняем пользователя с новыми правами
				DbSession.Save(ModifiedUser);
				var message = "Права добавлены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageUserPermission", "UserPermissions", new {ModifiedUser.Id});
			}
			ViewBag.PermissionsList =
				DbSession.Query<UserPermission>().Where(s => s != null).ToList()
					.Where(s => !ModifiedUser.Permissions.Contains(s))
					.OrderBy(s => s.Description)
					.ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.ModifiedUser = ModifiedUser;
			return View("ManageUserPermission", new {ModifiedUser.Id});
		}

		/// <summary>
		///     Удаление прав
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DeleteUserPermission([EntityBinder] ProducerUser ModifiedUser,
			[EntityBinder] UserPermission permission)
		{
			if (ModifiedUser == null || ModifiedUser.Id == 0 || permission == null || permission.Id == 0) {
				return RedirectToAction("ListUser");
			}
			ModifiedUser.Permissions.Remove(permission);
			var errors = ValidationRunner.Validate(ModifiedUser);
			if (errors.Count == 0) {
				// сохраняем админа с новыми правами
				DbSession.Save(ModifiedUser);
				var message = "Права удалены успешно";
				SuccessMessage(message);
				RedirectToAction("ManageUserPermission", "UserPermissions", new {ModifiedUser.Id});
			}
			ViewBag.PermissionsList =
				DbSession.Query<UserPermission>().Where(s => s != null).ToList()
					.Where(s => !ModifiedUser.Permissions.Contains(s))
					.OrderBy(s => s.Description)
					.ToList();
			ViewBag.CurrentPermissions = permission;
			ViewBag.ModifiedUser = ModifiedUser;
			return View("ManageUserPermission");
		}
	}
}