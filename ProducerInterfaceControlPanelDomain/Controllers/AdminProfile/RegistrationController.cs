using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.Web.Security;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class RegistrationController : MasterBaseController
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
				var X = DB.Account.Where(xxx => xxx.Login == login && xxx.TypeUser == 1).FirstOrDefault();

				if (X == null)
				{
					//  AddUserInDB(login);  // добавление обычного пользователя, у которого не будет прав
					AddAdminUserInBD(login); // добавление администратора
				}

				SetLoginCookie((object)login);
				return RedirectToAction("Index", "Home");
			}
			ErrorMessage("Неверно введён логин или пароль");
			return RedirectToAction("Index", "Registration");
		}

		public void AddUserInDB(string LogIn)
		{
			var CPU = new Account();

			CPU.Login = LogIn;
			CPU.EnabledEnum = UserStatus.Active;
			CPU.UserType = TypeUsers.ControlPanelUser;
			CPU.Appointment = "";

			if (_filterAttribute != null && _filterAttribute != "" && _filterAttribute.Length > 10)
				CPU.Name = _filterAttribute;

			DB.Account.Add(CPU);

			DB.SaveChanges();

			string AdminGroup = GetWebConfigParameters("Все");

			// TODO разобраться, почему нет типа и где его брать
			long IdGroup = DB.AccountGroup.Where(xxx => xxx.Name == AdminGroup).FirstOrDefault().Id;

			if (IdGroup > 0)
			{
				var Group = DB.AccountGroup.Where(xxx => xxx.Id == IdGroup).First();
				Group.Account.Add(CPU);
				DB.SaveChanges();
			}
			else
			{
				var Group = new AccountGroup();
				Group.Name = AdminGroup;
				Group.Account.Add(CPU);
			}

			DB.SaveChanges();

		}

		public void AddAdminUserInBD(string LogIn)
		{
			Account CPU = new Account();

			// CPU - ControlPanelUser

			CPU.Login = LogIn;
			CPU.EnabledEnum = UserStatus.Active;
			CPU.UserType = TypeUsers.ControlPanelUser;
			CPU.Appointment = "";

			if (_filterAttribute != null && _filterAttribute != "" && _filterAttribute.Length > 10)
			{
				CPU.Name = _filterAttribute;
			}

			DB.Account.Add(CPU);

			DB.SaveChanges();

			string AdminGroup = GetWebConfigParameters("AdminGroupName");

			var GroupExsist = DB.AccountGroup.Any(xxx => xxx.Name == AdminGroup && xxx.TypeGroup == SbyteTypeUser);

			if (GroupExsist)
			{
				var Group = DB.AccountGroup.Where(xxx => xxx.Name == AdminGroup && xxx.TypeGroup == SbyteTypeUser).First();
				Group.Account.Add(CPU);
			}
			else
			{
				var Group = new AccountGroup();
				Group.Name = AdminGroup;
				Group.Enabled = true;
				DB.AccountGroup.Add(Group);
				DB.SaveChanges();
				Group.Account.Add(CPU);
			}

			DB.SaveChanges();

		}

		public string ErrorMessageString;
		private string _filterAttribute;

		private DirectoryEntry GetDirectoryEntry(string login)
		{
			var entry = FindDirectoryEntry(login);
			if (entry == null)
				throw new Exception(String.Format("Учетная запись Active Directory {0} не найдена", login));
			return entry;
		}

		public void ChangePassword(string login, string password)
		{
			var entry = GetDirectoryEntry(login);
			GetDirectoryEntry(login).Invoke("SetPassword", password);
			entry.CommitChanges();
		}

		public void CreateUserInAD(string login, string password, bool allComputers = false)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
			//user.Properties["description"].Value = clientCode.ToString();
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.Properties["userAccountControl"].Value = 66048;
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
		}

		public DirectoryEntry FindDirectoryEntry(string login)
		{
			using (var searcher = new DirectorySearcher(String.Format(@"(&(objectClass=user)(sAMAccountName={0}))", login)))
			{
				var searchResult = searcher.FindOne();
				if (searchResult != null)
					return searcher.FindOne().GetDirectoryEntry();
				return null;
			}
		}

		public void SetLoginCookie(object login)
		{
			var ticket = new FormsAuthenticationTicket(
					1,
					login.ToString(),
					DateTime.Now,
					DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
					false,
					"",
					FormsAuthentication.FormsCookiePath);
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
			cookie.Name = GetWebConfigParameters("CookiesName");
			Response.Cookies.Set(cookie);
		}

		/// <summary>
		/// Выход
		/// </summary>
		/// <returns></returns>
		public ActionResult LogOut()
		{
			CurrentUser = null;
			LogOutUser();
			return RedirectToAction("Index");
		}

	}
}