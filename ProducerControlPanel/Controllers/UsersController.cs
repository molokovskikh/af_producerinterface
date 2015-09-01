using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	/// Страница управления пользователями
	/// </summary>
	[Authorize]
	public class UsersController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Пользователи";
		}

		public ActionResult Index()
		{
			ViewBag.UsersList = DbSession.Query<ProducerUser>().ToList();
			ViewBag.ProducerList = DbSession.Query<Producer>().ToList();
			ViewBag.AdminList = DbSession.Query<Admin>().ToList();
			return View();
		}
	}
}