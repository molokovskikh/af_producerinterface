using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceControlPanelDomain.Controllers.Global
{
	public class BaseController : ProducerInterfaceCommon.Controllers.BaseController
	{
		protected Account CurrentUser { get; set; }
		protected TypeUsers TypeLoginUser => TypeUsers.ControlPanelUser;
		protected sbyte SbyteTypeUser => (sbyte)TypeLoginUser;
		protected EmailSender Mails;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			CurrentUser = GetCurrentUser();
			SecurityCheck(CurrentUser, TypeLoginUser, filterContext);
			ViewBag.CurrentUser = CurrentUser;
			if (CurrentUser != null) {
				CurrentUser.IP = Request.UserHostAddress;
				Mails = new EmailSender(DB, CurrentUser);
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