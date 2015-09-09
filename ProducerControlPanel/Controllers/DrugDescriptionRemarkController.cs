using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using AnalitFramefork.Mvc.Attributes;
using NHibernate.Linq;
using ProducerInterface.Models;
using Remotion.Linq.Clauses;

namespace ProducerControlPanel.Controllers
{

	/// <summary>
	/// Страница управления пользователями
	/// </summary>
	[Description("Правки описаний препаратов")]
	public class DrugDescriptionRemarkController : BaseAdminController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Пользователи";
		}

		[Description("Список правок пользователей"), MainMenu]
		public ActionResult DrugDescriptionRemarkList()
		{
            var filter = new ModelFilter<DrugDescriptionRemark>(this);
			filter.GetCriteria();
			filter.SetOrderBy("Id", OrderingDirection.Desc);
			filter.Execute();
			ViewBag.Filter = filter;
            return View("DrugDescriptionRemarkList");
		}

		public ActionResult EditDrugDescriptionRemark(int id)
		{
			return SimpleEdit<DrugDescriptionRemark>(id);
		}

		[HttpPost]
		public ActionResult EditDrugDescriptionRemark(int id, bool accept)
		{
			if (accept)
				return AcceptDrugDescriptionRemark(id);
			return DeclineDrugDescriptionRemark(id);
		}

		[HttpPost]
		public ActionResult AcceptDrugDescriptionRemark(int id)
		{
			var remark = DbSession.Query<DrugDescriptionRemark>().First(i => i.Id == id);
			var admin = GetCurrentUser();
            remark.Apply(DbSession, admin);

			var errors = remark.GetErrors();
			if (errors.Length == 0) {
				SuccessMessage("Правка описания успешно применена");
				return Redirect(GetIndexActionUrl());
			}
			ErrorMessage(errors[0].Message);
			return RedirectToAction("EditDrugDescriptionRemark", new {id = id});
		}

		[HttpPost]
		public ActionResult DeclineDrugDescriptionRemark(int id)
		{
			var remark = DbSession.Query<DrugDescriptionRemark>().First(i => i.Id == id);
			var admin = GetCurrentUser();
            remark.Decline(DbSession, admin);
			var errors = remark.GetErrors();
			if (errors.Length == 0) {
				SuccessMessage("Правка описания успешно отклонена");
				return Redirect(GetIndexActionUrl());
			}
			ErrorMessage(errors[0].Message);
			return RedirectToAction("EditDrugDescriptionRemark", new {id = id});
		}
	}
}