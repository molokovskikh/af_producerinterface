using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Mvc;
using NHibernate.Linq;

namespace ProducerControlPanel.Controllers
{
	public class BaseAdminController : BaseController
	{
		//
		// GET: /BaseAdminController/

		[Description("Текущий пользователь")]
		private Admin CurrentUser { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			ViewBag.Admin = GetCurrentUser();

			ViewBag.JavascriptParams["baseurl"] = GetBaseUrl();
			ViewBag.ActionName = filterContext.RouteData.Values["action"].ToString();
			ViewBag.ControllerName = GetType().Name.Replace("Controller", "");
			ViewBag.Controller = this;
			//if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
			//{
			//	string loginUrl = Url.Action("Index", "AdminAccount"); // Default Login Url 
			//	filterContext.Result = new RedirectResult(loginUrl);
			//	return;
			//}

			//var currentUser = GetCurrentUser();
			//CrudListener.SetEmployee(currentUser.Id);
			//string name = ViewBag.ControllerName + "Controller_" + ViewBag.ActionName;
			//var permission = DbSession.Query<Permission>().FirstOrDefault(i => i.Name == name);
			////@todo убрать проверку, на accessDenined, а вместо этого просто не генерировать его. В целом подумать
			//if (permission != null && permission.Name != "AdminController_AccessDenined" && (currentUser == null || !currentUser.HasAccess(permission.Name)))
			//	filterContext.Result = new RedirectResult("/Admin/AccessDenined");

			//Если у контроллера, есть описательный аттрибут, то создаем хлебные крошки
			var hasDescription = Attribute.IsDefined(GetType(), typeof (DescriptionAttribute));
			if (hasDescription)
				SetBreadcrumb(this.GetDescription());
		}

		public Admin GetCurrentUser()
		{
			if (User == null || (User.Identity.Name == string.Empty) || DbSession == null || !DbSession.IsConnected) {
				CurrentUser = null;
				return null;
			}
			if (CurrentUser == null || User.Identity.Name != CurrentUser.UserName) {
				CurrentUser = DbSession.Query<Admin>().FirstOrDefault(e => e.UserName == User.Identity.Name);
			}
			return CurrentUser;
		}
	}
}