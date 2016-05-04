using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.Data.Entity;
using ProducerInterfaceCommon.ViewModel.Interface.Profile;

namespace ProducerInterface.Controllers
{
	public class AccountAuthController : MasterBaseController
	{

		/// <summary>
		/// Вход
		/// </summary>
		/// <param name="user">заполненная форма логин-пароль</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UserAuthentication(LoginValidation user)
		{
			// валидация
			if (String.IsNullOrEmpty(user.login) || String.IsNullOrEmpty(user.password))
			{
				ErrorMessage("Некорректно введены данные. Вашим логином является email, указанный при регистрации. Пароль при регистрации был выслан на ваш email");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// проверка наличия в БД
			var thisUser = cntx_.Account.SingleOrDefault(x => x.Login == user.login && x.TypeUser == SbyteTypeUser);
			if (thisUser == null)
			{
				ErrorMessage("Пользователь не найден. Вашим логином является email, указанный при регистрации");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// проверка пароля
			var passHash = Md5HashHelper.GetHash(user.password);
			if (passHash != thisUser.Password)
			{
				ErrorMessage("Неправильно введен пароль");
				ViewBag.CurrentUser = null;
				return RedirectToAction("Auth", "Account");
			}

			// если логинится не впервый раз и не заблокирован
			if (thisUser.Enabled == 1)
			{
				CurrentUser = thisUser;
				return Autentificate();
			}

			// если логинится впервые
			else if (thisUser.Enabled == 0 && !thisUser.PasswordUpdated.HasValue) 
			{
				var otherUsersExists = cntx_.Account.Any(x => x.CompanyId == thisUser.CompanyId.Value && x.Enabled == 1 && x.TypeUser == SbyteTypeUser);
				// если других пользователей этой компании нет - добавляем пользователя в группу администраторов
				if (!otherUsersExists)
				{
					// если группы администраторов нет - создаем ее
					var adminGroupName = GetWebConfigParameters("AdminGroupName");
					var adminGroup = cntx_.AccountGroup.SingleOrDefault(x => x.Name == adminGroupName && x.TypeGroup == SbyteTypeUser);
					if (adminGroup == null)
					{
						adminGroup = new AccountGroup() { Name = adminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
						cntx_.Entry(adminGroup).State = EntityState.Added;
						cntx_.SaveChanges();
					}
					adminGroup.Account.Add(thisUser);
					cntx_.SaveChanges();
				}
				// если есть другие пользователи компании - включаем в группу Все пользователи
				else
				{
					var otherGroupName = GetWebConfigParameters("LogonGroupAcess");
					var otherGroup = cntx_.AccountGroup.SingleOrDefault(x => x.Name == otherGroupName && x.TypeGroup == SbyteTypeUser);
					if (otherGroup == null) {
						otherGroup = new AccountGroup() { Name = otherGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
						cntx_.Entry(otherGroup).State = EntityState.Added;
						cntx_.SaveChanges();
					}
					otherGroup.Account.Add(thisUser);
					cntx_.SaveChanges();
				}

				thisUser.PasswordUpdated = DateTime.Now;
				thisUser.Enabled = 1;
				cntx_.Entry(thisUser).State = EntityState.Modified;
				cntx_.SaveChanges();

				CurrentUser = thisUser;
				SuccessMessage("Вы успешно подтвердили свою регистрацию на сайте");
				return Autentificate();
			}

			// Аккаунт заблокирован
			else if (thisUser.Enabled == 0 && thisUser.PasswordUpdated.HasValue)
			{
					CurrentUser = null;
					ErrorMessage("Ваш аккаунт заблокирован");
					return RedirectToAction("Index", "Home");
			}

			return RedirectToAction("Index", "Home");
		}

		public ActionResult Autentificate(string userData = "")
		{
			SetUserCookiesName(CurrentUser.Login, true, userData);
			SetCookie("AccountName", CurrentUser.Login, false);
			return RedirectToAction("Index", "Profile");
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
				ErrorMessage($"Пользователь с email {login} не найден, обращайтесь на {GetWebConfigParameters("MailFrom")}");
				return RedirectToAction("PasswordRecovery", "Account", new { eMail = login });
			}

			// пользователь зарегистрировался, но ни разу не входил: отсылаем новый пароль на почту
			if (user.Enabled == 0 && !user.PasswordUpdated.HasValue)
			{
				var password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				cntx_.Entry(user).State = EntityState.Modified;
				cntx_.SaveChanges();
				EmailSender.SendPasswordRecoveryMessage(cntx_, user.Id, password, Request.UserHostAddress);

				SuccessMessage("Новый пароль отправлен на ваш email: " + login);
			}

			// если незаблокирован: отсылаем новый пароль на почту
			if (user.Enabled == 1)
			{
				var password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				cntx_.Entry(user).State = EntityState.Modified;
				cntx_.SaveChanges();
				EmailSender.SendPasswordRecoveryMessage(cntx_, user.Id, password, Request.UserHostAddress);

				SuccessMessage("Новый пароль отправлен на ваш email " + login);
			}

			// если заблокирован
			if (user.Enabled == 0 && !user.PasswordUpdated.HasValue)
				ErrorMessage("Ваша учетная запись заблокирована, обращайтесь на " + GetWebConfigParameters("MailFrom"));

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult ChangePassword(ChangePassword NewPassword)
		{
			if (!ModelState.IsValid)
				return View(NewPassword);

			var user = cntx_.Account.First(x => x.Login == CurrentUser.Login && x.TypeUser == 0);
			user.Password = Md5HashHelper.GetHash(NewPassword.Pass);
			cntx_.Entry(user).State = EntityState.Modified;
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

		/// <summary>
		/// Выход
		/// </summary>
		/// <returns></returns>
		public ActionResult LogOut()
		{
			// зануляем куки регистрации формой
			LogOutUser();
			// возвращаем пользователя на главную страницу
			return RedirectToAction("Index", "Home");
		}
	}
}