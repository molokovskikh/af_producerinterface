using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Analit.Components;
using AnalitFramefork.Components;
using AnalitFramefork.Helpers;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	public class RegistrationController : BaseInterfaceController
	{
		//todo: перенести из админки всплывающее окно сообщения, добавить отображение валидации полей.
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			if (filterContext == null) {
				throw new ArgumentNullException("filterContext");
			}
		}

		//
		// GET: /Registration/
		// Форма авторизации / регистрации
		public ActionResult Index()
		{
			ViewBag.ProducerList = DbSession.Query<Producer>().ToList();
			ViewBag.CurrentUser = DbSession.Query<User>().FirstOrDefault(e => e.Name == User.Identity.Name);
			return View();
		}

		/// <summary>
		/// Регистрация пользователя
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public ActionResult RegisterUser(User user)
		{
			// Проверяем введенные пользователем данные регистрации 
			string resultMessage = "";
			if (user.RegistrationIsAllowed(DbSession, ref resultMessage)) {
				// хэшируем пароль
				user.Password = Md5HashHelper.GetHash(user.Password); 
				user.PasswordUpdated = SystemTime.Now();
				// сохраняем модель нового пользователя 
				DbSession.Save(user);
				var linkWord = Md5HashHelper.GetHash(user.PasswordUpdated.ToString());
				// письмо пользователю
				EmailSender.SendEmail(user.Email, "Успешная регистрация на сайте " + Config.GetParam("SiteName"),
					string.Format(@"Вы успешно зарегистрировались на сайте {0} под логином {1},
									для завершение регистрации перейдите по <a href='{2}'>ссылке</a>.",
						Config.GetParam("SiteName"), user.Login, Config.GetParam("SiteRoot") + "Registration/GegistrationConfirm?key=" + linkWord));
				// письмо письма на аналит 
				EmailSender.SendEmail(Config.GetParam("AnalitEmail"), "Успешная регистрация на сайте " + Config.GetParam("SiteName"),
					string.Format(@"Вы успешно зарегистрировались на сайте {0} под логином {1},
									для завершение регистрации перейдите по <a href='{2}'>ссылке</a>.",
						Config.GetParam("SiteName"), user.Login, Config.GetParam("SiteRoot") + "Registration/GegistrationConfirm?key=" + linkWord));
				SuccessMessage(resultMessage);
				return RedirectToAction("Index", "Home");
			}
			ErrorMessage(resultMessage);
			ViewBag.NewUser = user;
			ViewBag.ProducerList = DbSession.Query<Producer>().ToList();
			return View("Index");
		}
		 
		/// <summary>
		/// Подтверждение регистрации пользователя по ссылке в письме
		/// </summary>
		/// <returns></returns>
		public ActionResult GegistrationConfirm(string key)
		{
			var currentUserList = DbSession.Query<User>().ToList();
			var currentUser = currentUserList.FirstOrDefault(e => Md5HashHelper.GetHash(e.PasswordUpdated.ToString()) == key);
			if (currentUser != null) {
				currentUser.Enabled = true;
				currentUser.PasswordUpdated = SystemTime.Now();
				currentUser.PasswordToUpdate = false;
				DbSession.Save(currentUser);
				return Authenticate("Index", "Home", currentUser.Name, true);
			}
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// Авторизация пользователя (аутентификация)
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UserAuthentication(string login, string password)
		{
			var redirectToAction = "Index";
			var redirectToControll = "Home";
			// Проверяем введенные пользователем данные авторизации 
			password = Md5HashHelper.GetHash(password);
			var authenticatedUser = DbSession.Query<User>().FirstOrDefault(s => s.Login == login
			                                                                    && s.Password == password && (s.Enabled || s.Enabled == false && s.PasswordToUpdate));
			if (authenticatedUser != null) {
				if (authenticatedUser.PasswordToUpdate) {
					redirectToAction = "UserPasswordUpdate";
					redirectToControll = "Registration";
					ViewBag.CurrentUser = authenticatedUser;
					return View("UserPasswordUpdate");
				}
				// авторизуем пользователя, если все есть совпадение в БД
				return Authenticate(redirectToAction, redirectToControll, authenticatedUser.Name, true);
			}
			ErrorMessage("Пользователь с данным логином и паролем не существует или был заблокирован.");
			// возвращаем пользователя на главную страницу
			return RedirectToAction("Index", "Home");
		}
		/// <summary>
		/// Обновление пароля пользователя.
		/// </summary>
		/// <returns></returns>
		public ActionResult UserPasswordUpdate()
		{
			ViewBag.CurrentUser = DbSession.Query<User>().FirstOrDefault(e => e.Name == User.Identity.Name);
			return View();
		}


		/// <summary>
		/// Обновление пароля пользователя.
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UserPasswordUpdate(string passwordOld, string passwordNew, string passwordProof)
		{
			int currentUserId = 0;
			Int32.TryParse(Request.Form["UserNumber"], out currentUserId);
			var currentUser = DbSession.Query<User>().FirstOrDefault(e => e.Name == User.Identity.Name || e.Id == currentUserId);
			ViewBag.CurrentUser = currentUser;
			if (currentUser != null) {
				if (currentUser.Password != Md5HashHelper.GetHash(passwordOld)) {
					ErrorMessage("Введенный старый пароль не совпадает с действительным.");
					return View();
				}
				if (passwordNew != passwordProof) {
					ErrorMessage("Введенный новый пароль не совпадает с его подтверждением.");
					return View();
				}
				if (passwordNew.Length < 5) {
					ErrorMessage("Новый пароль должен быть длиннее 5 символов.");
					return View();
				}
				currentUser.UpdatePassword(DbSession, passwordNew);
				currentUser.Enabled = true;
				currentUser.PasswordToUpdate = false;
				DbSession.Save(currentUser);
				ErrorMessage("На указанную почту было выслано сообщение с новым паролем.");
				return Authenticate("Index", "Home", currentUser.Name, true);
			}
			ErrorMessage("Пользователя с введенным email адресом не существует.");
			return View();
		}

		/// <summary>
		/// Изменение пароля пользователя на временный
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UserPasswordUpdateRandom(string email)
		{
			var authenticatedUser = DbSession.Query<User>().FirstOrDefault(s => s.Email == email);
			if (authenticatedUser != null) {
				authenticatedUser.UpdatePasswordByDefault(DbSession);
				authenticatedUser.Enabled = false;
				authenticatedUser.PasswordToUpdate = true;
				DbSession.Save(authenticatedUser); 
				ErrorMessage("На указанную почту было выслано сообщение с новым паролем.");
				return RedirectToAction("Index", "Home");
			}
			ErrorMessage("Пользователя с введенным email адресом не существует.");
			return RedirectToAction("UserPasswordUpdate");
		}

		/// <summary>
		/// Выход из профиля пользователя , сброс куки
		/// </summary>
		/// <returns></returns>
		public ActionResult LogOut()
		{
			// зануляем куки регистрации формой
			SetCookie(FormsAuthentication.FormsCookieName, null);
			// возвращаем пользователя на главную страницу
			return RedirectToAction("Index", "Home");
		}
	}
}