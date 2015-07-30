using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerControlPanel.Models;
using ProducerInterface.Controllers;
using ProducerInterface.Models;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	/// Главная
	/// </summary>
	[Authorize]
	public class AdminController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{ 
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Главная";
		}

		public ActionResult Index()
		{
			ViewBag.Admin = DbSession.Query<Admin>().FirstOrDefault(e => e.UserName == User.Identity.Name);
			return View();
		}
	}
}