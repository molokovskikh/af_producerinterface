using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class RegistrationController : ProducerInterfaceCommon.Controllers.BaseController
	{
		// GET: Registration
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(string login, string password)
		{
			var Login_ = login.Split(new Char[] { '@' }).First();
			login = Login_;

			var DomainAuth = new ProducerInterfaceCommon.Controllers.DomainAutentification();

			if (DomainAuth.IsAuthenticated(login, password))
			{
				var X = DB.Account.FirstOrDefault(x => x.Login == login && x.TypeUser == 1);

				if (X == null)
				{
					AddAdminUserInBD(login); // добавление администратора
				}

				FormsAuthentication.SetAuthCookie(login, true);
				return RedirectToAction("Index", "Home");
			}
			ErrorMessage("Неверно введён логин или пароль");
			return RedirectToAction("Index", "Registration");
		}

		public void AddAdminUserInBD(string LogIn)
		{
			var account = new Account {
				Login = LogIn,
				EnabledEnum = UserStatus.Active,
				UserType = TypeUsers.ControlPanelUser,
				Appointment = ""
			};

			DB.Account.Add(account);
			DB.SaveChanges();

			string AdminGroup = GetWebConfigParameters("AdminGroupName");

			var GroupExsist = DB.AccountGroup.Any(xxx => xxx.Name == AdminGroup && xxx.TypeGroup == (sbyte)TypeUsers.ControlPanelUser);

			if (GroupExsist)
			{
				var Group = DB.AccountGroup.Where(xxx => xxx.Name == AdminGroup && xxx.TypeGroup == (sbyte)TypeUsers.ControlPanelUser).First();
				Group.Account.Add(account);
			}
			else
			{
				var Group = new AccountGroup();
				Group.Name = AdminGroup;
				Group.Enabled = true;
				DB.AccountGroup.Add(Group);
				DB.SaveChanges();
				Group.Account.Add(account);
			}

			DB.SaveChanges();

		}

		public ActionResult LogOut()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Index");
		}
	}
}