using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers.Global
{
		public class BaseController : ProducerInterfaceCommon.Controllers.BaseController
		{
			protected Account CurrentUser { get; set; }
			protected TypeUsers TypeLoginUser => TypeUsers.ControlPanelUser;
			protected sbyte SbyteTypeUser => (sbyte)TypeLoginUser;

			protected override void OnActionExecuting(ActionExecutingContext filterContext)
			{
				base.OnActionExecuting(filterContext);
				CurrentUser = GetCurrentUser();
				SecurityCheck(CurrentUser, TypeLoginUser, filterContext);
				ViewBag.CurrentUser = CurrentUser;
				if (CurrentUser != null) {
					CurrentUser.IP = Request.UserHostAddress;
				}
			}

			private Account GetCurrentUser()
			{
				var login = HttpContext.User.Identity.Name;
				if (String.IsNullOrEmpty(login))
					return null;

				return DB.Account
					.FirstOrDefault(x => x.TypeUser == (sbyte)TypeLoginUser && x.Login == login && x.Enabled == (sbyte)UserStatus.Active);
			}
		}
}