using System;
using System.Web.Mvc;
using System.Linq;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Controllers;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface.Controllers
{
	public class BaseController : ProducerInterfaceCommon.Controllers.BaseController
	{
		protected Account CurrentUser { get; set; }
		protected Account CurrentAdmin { get; set; }
		protected sbyte SbyteTypeUser => (sbyte)TypeUsers.ProducerUser;
		protected EmailSender Mails;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			CurrentUser = GetCurrentUser();
			Mails = new EmailSender(DB, DB2, CurrentUser);
			Mails.IP = Request.UserHostAddress;
			SecurityCheck(CurrentUser, TypeUsers.ProducerUser, filterContext);

			if (CurrentUser != null)
			{
				CurrentAdmin = GetCurrentAdmin();
				CurrentUser.ID_LOG = CurrentAdmin?.Id ?? CurrentUser.Id;
				CurrentUser.IP = Request.UserHostAddress;
				if (CurrentUser.AccountCompany.ProducerId != null)
					ViewBag.Producernames = DB.producernames.Single(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId).ProducerName;
				else
					ViewBag.Producernames = "Физическое лицо";
				ViewBag.CurrentUser = CurrentUser;
				ViewBag.AdminUser = CurrentAdmin;
			}
		}

		private Account GetCurrentAdmin()
		{
			var cookie = Request.Cookies.Get("auth");
			if (cookie == null)
				return null;
			string content;
			try {
				content = FormsAuthentication.Decrypt(cookie.Value)?.Name;
			} catch(Exception) {
				return null;
			}
			long value;
			if (long.TryParse(content, out value))
			{
				return DB.Account
					.FirstOrDefault(x => x.TypeUser == (sbyte)TypeUsers.ControlPanelUser && x.Id == value && x.Enabled == (sbyte)UserStatus.Active);
			}
			return null;
		}

		private Account GetCurrentUser()
		{
			var login = HttpContext.User.Identity.Name;
			if (String.IsNullOrEmpty(login))
				return null;

			return DB.Account
				.FirstOrDefault(x => x.TypeUser == (sbyte)TypeUsers.ProducerUser && x.Login == login && x.Enabled == (sbyte)UserStatus.Active);
		}
	}
}
