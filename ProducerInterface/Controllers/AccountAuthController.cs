using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface.Controllers
{
	public class AccountAuthController : MasterBaseController
	{

		[HttpPost]
		public ActionResult UserAuthentication(LoginValidation User_)
		{

			if (String.IsNullOrEmpty(User_.login) && String.IsNullOrEmpty(User_.password))
			{
				ErrorMessage("Некорректно введены данные.  Вашим логином является Email, указанный при регистрации. Пароль при регистрации был выслан на ваш Email");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// валидация

			var ThisUser = cntx_.Account.ToList().Where(xxx => xxx.Login.ToLower() == User_.login.ToLower() && xxx.TypeUser == SbyteTypeUser).FirstOrDefault();

			if (ThisUser == null || ThisUser.Login == "")
			{
				ErrorMessage("Пользователь с данным Логином не существует. Вашим логином является Email, указанный при регистрации.");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// проверка наличия в БД

			string originalPass = User_.password;
			User_.password = Md5HashHelper.GetHash(User_.password);

			if (User_.password != ThisUser.Password)
			{
				ErrorMessage("Неправильно введен пароль.");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// проверка пароля

			// если всё выше перечисленное пройдено,
			// проверяем первый раз логинится пользователь или второй или после смены пароля

			if (ThisUser.Enabled == 1) // логинится не впервый раз и не заблокирован
			{

				CurrentUser = ThisUser as Account;
				return Autentificate(this, shouldRemember: true);
			}


			if (ThisUser.Enabled == 0 && !ThisUser.PasswordUpdated.HasValue) // логинится впервые
			{

				var ListUser = cntx_.Account.Where(xxx => xxx.CompanyId == ThisUser.CompanyId.Value).Where(xxx => xxx.Enabled == 1 && xxx.TypeUser == SbyteTypeUser).ToList();

				List<AccountPermission> LST = cntx_.AccountPermission.ToList();

				if (ListUser == null || ListUser.Count() == 0)
				{
					// данный пользователь зарегистрировался первым, даём ему все права

					// пользователь зарегистрировался первым, добавляем его в группу администраторов

					string AdminGroupName = GetWebConfigParameters("AdminGroupName");

					// проверяем наличие группы администраторов


					var AdminGroup = cntx_.AccountGroup.Where(xxx => xxx.Name == AdminGroupName).FirstOrDefault();

					if (String.IsNullOrEmpty(AdminGroup.Name))
					{
						AdminGroup = new AccountGroup();

						AdminGroup.Name = AdminGroupName;
						AdminGroup.Enabled = true;
						AdminGroup.Description = "Администраторы";
						AdminGroup.TypeGroup = SbyteTypeUser;
						cntx_.Entry(AdminGroup).State = System.Data.Entity.EntityState.Added;
						cntx_.SaveChanges();
					}

					AdminGroup.Account.Add(ThisUser);
					cntx_.SaveChanges();
				}
				else
				{

					// Даём ему права для входа в ЛК
					// LogonGroupAcess 

					var GroupName = GetWebConfigParameters("LogonGroupAcess");

					var OtherGroup = cntx_.AccountGroup.Where(xxx => xxx.Name == GroupName && xxx.TypeGroup == SbyteTypeUser).FirstOrDefault();


					if (String.IsNullOrEmpty(OtherGroup.Name))
					{
						OtherGroup = new AccountGroup();

						OtherGroup.Name = GroupName;
						OtherGroup.Enabled = true;
						OtherGroup.Description = "Администраторы";
						OtherGroup.TypeGroup = SbyteTypeUser;
						cntx_.Entry(OtherGroup).State = System.Data.Entity.EntityState.Added;
						cntx_.SaveChanges();
					}

					OtherGroup.Account.Add(ThisUser);
					cntx_.SaveChanges();
				}
				SuccessMessage("Вы успешно подтвердили свою регистрацию на сайте");

				ThisUser.PasswordUpdated = DateTime.Now;
				ThisUser.Enabled = 1;
				cntx_.Entry(ThisUser).State = System.Data.Entity.EntityState.Modified;

				cntx_.SaveChanges();

				//CurrentUser = ThisUser;
				//ViewBag.CurrentUser = ThisUser;
				CurrentUser = ThisUser;
				return Autentificate(this, shouldRemember: true);
			}

			if (ThisUser.Enabled == 0 && ThisUser.PasswordUpdated.HasValue)
			{
				if (ThisUser.PasswordUpdated == null)
				{
					// Восстановление пароля

					ThisUser.Enabled = 1;
					ThisUser.PasswordUpdated = DateTime.Now;
					cntx_.Entry(ThisUser).State = System.Data.Entity.EntityState.Modified;
					cntx_.SaveChanges();

					//CurrentUser = ThisUser;
					//ViewBag.CurrentUser = ThisUser;
					CurrentUser = ThisUser;
					return Autentificate(this, shouldRemember: true);
				}
				else
				{
					// Аккаунт заблокирован

					ErrorMessage("Аккаунт заблокирован");
					//CurrentUser = null;
					//ViewBag.CurrentUser = null;
					CurrentUser = null;
					ErrorMessage("Аккаунт заблокирован");
					return RedirectToAction("Index", "Home");
				}
			}


			// CurrentUser = null;
			// ViewBag.CurrentUser = null;
			// AutorizedUser = null;
			return RedirectToAction("Index", "Home");
		}

		public ActionResult Autentificate(Controller currentController, bool shouldRemember, string userData = "")
		{
			string autorizeddd = Autentificates(this, CurrentUser.Login, shouldRemember, userData);
			string controllerName = (autorizeddd.Split(new Char[] { '/' }))[0];
			string actionName = (autorizeddd.Split(new Char[] { '/' }))[1];
			return RedirectToAction(actionName, controllerName);
		}

		public string Autentificates(Controller CRT, string username, bool shouldRemember, string userData = "")
		{
			var currentUser = cntx_.Account.Where(x => x.Login == username && x.TypeUser == (sbyte)TypeUsers.ProducerUser).First();
			AutorizeCurrentUser(currentUser, TypeUsers.ProducerUser, userData);

			return "Profile/index";
		}


		/// <summary>
		/// Форма ввода email для напоминания пароля
		/// </summary>
		/// <param name="eMail">Подстановка email</param>
		/// <returns></returns>
		public ActionResult PasswordRecovery(string eMail = "")
		{
			var model = new PasswordUpdate();
			model.login = eMail;
			return View("PasswordRecovery", model);
		}

		/// <summary>
		/// Напоминание пароля
		/// </summary>
		/// <param name="login">введенный логин (=email)</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult PasswordRecoverySendMail(string login = "")
		{
			if (String.IsNullOrEmpty(login))
			{
				ErrorMessage("Вы не указали eMail");
				return RedirectToAction("PasswordRecovery", "Account");
			}

			var user = cntx_.Account.FirstOrDefault(x => x.Login == login && x.TypeUser == 0);

			// пользователь не найден, отсылаем на домашнюю, с ошибкой
			if (user == null)
			{
				ErrorMessage("Пользователь с email " + login + " не найден, обращайтесь на " + System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString());
				return RedirectToAction("PasswordRecovery", "Account", new { eMail = login });
			}

			// пользователь зарегистрировался, но ни разу не входил: отсылаем новый пароль на почту
			if (user.Enabled == 0 && !user.PasswordUpdated.HasValue)
			{
				string password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				cntx_.Entry(user).State = System.Data.Entity.EntityState.Modified;
				cntx_.SaveChanges();
				EmailSender.SendPasswordRecoveryMessage(cntx_, user.Id, password, Request.UserHostAddress);

				SuccessMessage("Новый пароль отправлен на ваш email: " + login);
				return RedirectToAction("Index", "Home");
			}

			// если незаблокирован: отсылаем новый пароль на почту
			if (user.Enabled == 1)
			{
				string password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				cntx_.Entry(user).State = System.Data.Entity.EntityState.Modified;
				cntx_.SaveChanges();
				EmailSender.SendPasswordRecoveryMessage(cntx_, user.Id, password, Request.UserHostAddress);

				SuccessMessage("Новый пароль отправлен на ваш email: " + login);
				return RedirectToAction("Index", "Home");
			}

			// если заблокирован
			ErrorMessage("Ваша учетная запись Заблокирована, для обращений используйте email " + System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString());
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult ChangePassword(ProducerInterfaceCommon.ViewModel.Interface.Profile.ChangePassword NewPassword)
		{
			if (!ModelState.IsValid)
			{
				return View(NewPassword);
			}

			var user = cntx_.Account.First(x => x.Login == CurrentUser.Login && x.TypeUser == 0);
			user.Password = Md5HashHelper.GetHash(NewPassword.Pass);

			cntx_.Entry(user).State = System.Data.Entity.EntityState.Modified;
			cntx_.SaveChanges();
			EmailSender.SendPasswordChangeMessage(cntx: cntx_, userId: user.Id, password: NewPassword.Pass, ip: Request.UserHostAddress);

			SuccessMessage("Новый пароль сохранен и отправлен на ваш email: " + user.Login);
			return RedirectToAction("Index", "Profile");
		}

		[HttpGet]
		public ActionResult ChangePassword()
		{
			return View(new ProducerInterfaceCommon.ViewModel.Interface.Profile.ChangePassword());
		}

		public ActionResult LogOut()
		{
			// зануляем куки регистрации формой
			LogOut_User();
			// возвращаем пользователя на главную страницу
			return RedirectToAction("Index", "Home");
		}
	}
}