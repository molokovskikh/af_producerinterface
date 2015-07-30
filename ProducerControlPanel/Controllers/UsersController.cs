using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerControlPanel.Models;
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
			ViewBag.UsersList = DbSession.Query<ProducerInterface.Models.User>().ToList();
			ViewBag.ProducerList = DbSession.Query<ProducerInterface.Models.Producer>().ToList();
			ViewBag.AdminList = DbSession.Query<Admin>().ToList();
			return View();
		}
	}
}