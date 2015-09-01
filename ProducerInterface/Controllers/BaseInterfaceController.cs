using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AnalitFramefork.Extensions;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	public class BaseInterfaceController : BaseController
	{
		//
		// GET: /BaseInterfaceController/

		[Description("Текущий пользователь")]
		private ProducerUser CurrentProducerUser { get; set; }

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.JavascriptParams["baseurl"] = GetBaseUrl();
			ViewBag.ActionName = filterContext.RouteData.Values["action"].ToString();
			ViewBag.ControllerName = GetType().Name.Replace("Controller", "");
			ViewBag.CurrentUser = GetCurrentUser();
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

		public ProducerInterface.Models.ProducerUser GetCurrentUser()
		{
			if (User == null || (User.Identity.Name==String.Empty) || DbSession == null || !DbSession.IsConnected) {
				CurrentProducerUser = null;
				return null;
			}
			if (CurrentProducerUser == null || User.Identity.Name != CurrentProducerUser.Name) {
				CurrentProducerUser = DbSession.Query<ProducerInterface.Models.ProducerUser>().FirstOrDefault(e => e.Name == User.Identity.Name);
			}
			return CurrentProducerUser;
		}
	}
}