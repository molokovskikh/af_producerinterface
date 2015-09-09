using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	///     Роли администраторов
	/// </summary> 
	public class AdminRolesController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Роли";
		}

		/// <summary>
		///     Список ролей
		/// </summary>
		/// <returns></returns> 
		public ActionResult ListRoles()
		{
			var pager = new ModelFilter<AdminRole>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Просмотр ролей
		/// </summary>
		public ActionResult InfoRole(int id)
		{
			var profileRole = DbSession.Query<AdminRole>().FirstOrDefault(s => s.Id == id);
			if (profileRole == null) {
				return RedirectToAction("ListRoles");
			}
			ViewBag.CurrentRole = profileRole;
			return View();
		}

		/// <summary>
		///     Создание роли
		/// </summary>
		public ActionResult CreateRole()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList();
			return View();
		}

		[HttpPost]
		public ActionResult CreateRole([EntityBinder] AdminRole role)
		{
			var errors = ValidationRunner.Validate(role);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(role);
				var message = "Новость добавлена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListRoles");
			}
			ViewBag.CurrentRole = role;
			return View();
		}

		/// <summary>
		///     Редактирование роли
		/// </summary>
		public ActionResult EditRole(int id)
		{
			var role = DbSession.Query<AdminRole>().FirstOrDefault(s => s.Id == id);
			if (role == null) {
				return RedirectToAction("ListRoles");
			}
			ViewBag.CurrentRole = role;
			return View();
		}

		[HttpPost]
		public ActionResult EditRole([EntityBinder] AdminRole role)
		{
			var errors = ValidationRunner.Validate(role);
			if (errors.Count == 0) {
				// сохраняем модель нового пользователя 
				DbSession.Save(role);
				var message = "Новость изменена успешно";
				SuccessMessage(message);
				return RedirectToAction("ListRoles");
			}
			ViewBag.CurrentRole = role;
			return View();
		}

		/// <summary>
		///     Удаление роли
		/// </summary>
		public ActionResult DeleteRole(int id)
		{
			var currentRole = DbSession.Query<AdminRole>().FirstOrDefault(s => s.Id == id);
			if (DbSession.AttemptDelete(currentRole)) {
				var message = "Роль удалена успешно";
				SuccessMessage(message);
			}
			else {
				var message = "Роль не может быть удалена";
				ErrorMessage(message);
			}
			return RedirectToAction("ListRoles");
		}
	}
}