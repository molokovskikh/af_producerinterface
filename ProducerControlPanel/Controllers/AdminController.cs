using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerControlPanel.Models;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{
	/// <summary>
	///     Главная
	/// </summary>
	[Description("Панель администратора"), MainMenu]
	public class AdminController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Главная";
		}

		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		///     Список логов
		/// </summary>
		/// <returns></returns>
		[Description("Список логов админов"), MainMenu]
		public ActionResult LogRegAdminResultList()
		{
			var pager = new ModelFilter<AdminLogModel>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Информация по логам
		/// </summary>
		public ActionResult LogRegAdminResultInfo(int Id = 0, string path = "")
		{
			var log = DbSession.Query<AdminLogModel>().FirstOrDefault(s => s.Id == Id);
			ViewBag.Log = log;
			ViewBag.BackUrl = (path == string.Empty ? "/Admin/LogRegAdminResultList" : path);
			return View();
		}

		/// <summary>
		///     Список логов
		/// </summary>
		/// <returns></returns>
		[Description("Список логов пользователей"), MainMenu]
		public ActionResult LogRegUserResultList()
		{
			var pager = new ModelFilter<UserLogModel>(this);
			if (pager.GetParam("orderBy") == null) {
				pager.SetOrderBy("Id", OrderingDirection.Desc);
			}
			pager.GetCriteria();
			ViewBag.Pager = pager;
			return View();
		}

		/// <summary>
		///     Информация по логам
		/// </summary>
		public ActionResult LogRegUserResultInfo(int Id = 0, string path = "")
		{
			var log = DbSession.Query<UserLogModel>().FirstOrDefault(s => s.Id == Id);
			ViewBag.Log = log;
			ViewBag.BackUrl = (path == string.Empty ? "/Admin/LogRegUserResultList" : path);
			return View();
		}
	}
}