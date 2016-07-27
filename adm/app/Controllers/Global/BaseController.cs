using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using NHibernate;

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
				Mails = new EmailSender(DB, DB2, CurrentUser);
			}
		}

		private Account GetCurrentUser()
		{
			var login = HttpContext.User.Identity.Name;
#if DEBUG
			login = Request.QueryString["debug-user"] ?? Request.Cookies["debug-user"]?.Value ?? login;
			if (Request.QueryString["debug-user"] != null)
				Response.Cookies.Add(new HttpCookie("debug-user", login));
#endif
			if (String.IsNullOrEmpty(login))
				return null;

			var user = DB.Account.FirstOrDefault(x => x.TypeUser == (sbyte)TypeLoginUser
				&& x.Login == login && x.Enabled == (sbyte)UserStatus.Active);
			return user;
		}
	}
}