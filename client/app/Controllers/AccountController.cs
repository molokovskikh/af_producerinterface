﻿using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Registration;
using ProducerInterfaceCommon.Controllers;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ViewModel.Interface.Profile;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web;
using System.Web.Security;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterface.Controllers
{
	public class AccountController : BaseController
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
				return Redirect("~");
			}

			// проверка наличия в БД
			var thisUser = DB.Account.SingleOrDefault(x => x.Login == user.login && x.TypeUser == SbyteTypeUser);
			if (thisUser == null)
			{
				ErrorMessage("Пользователь не найден. Вашим логином является email, указанный при регистрации");
				ViewBag.CurrentUser = null;
				return Redirect("~");
			}

			// проверка пароля
			var passHash = Md5HashHelper.GetHash(user.password);
			if (passHash != thisUser.Password)
			{
				ErrorMessage("Неправильно введен пароль");
				ViewBag.CurrentUser = null;
				return Redirect("~");
			}

			// если логинится не впервый раз и не заблокирован
			if (thisUser.EnabledEnum == UserStatus.Active)
			{
				CurrentUser = thisUser;
				return Autentificate();
			}

			// если логинится впервые
			else if (thisUser.EnabledEnum == UserStatus.New)
			{
				var group = DB.AdminGroup();
				group.Account.Add(thisUser);
				DB.SaveChanges();

				thisUser.PasswordUpdated = DateTime.Now;
				thisUser.EnabledEnum = UserStatus.Active;
				DB.Entry(thisUser).State = EntityState.Modified;
				DB.SaveChanges();

				CurrentUser = thisUser;
				SuccessMessage("Вы успешно подтвердили свою регистрацию на сайте");
				return Autentificate();
			}

			// аккаунт заблокирован
			else if (thisUser.EnabledEnum == UserStatus.Blocked)
			{
				CurrentUser = null;
				ErrorMessage("Ваша учетная запись заблокирована, обращайтесь на " + ConfigurationManager.AppSettings["MailFrom"]);
				return Redirect("~");
			}

			// заявка на регистрацию
			else if (thisUser.EnabledEnum == UserStatus.Request)
			{
				CurrentUser = null;
				SuccessMessage("Ваша заявка на регистрацию еще не рассмотрена, обращайтесь на " + ConfigurationManager.AppSettings["MailFrom"]);
				return Redirect("~");
			}
			return Redirect("~");
		}

		public ActionResult Autentificate(long adminId = 0)
		{
			FormsAuthentication.SetAuthCookie(CurrentUser.Login, true);
			if (adminId > 0) {
				var ticket = new FormsAuthenticationTicket(adminId.ToString(), true, (int)FormsAuthentication.Timeout.TotalMinutes);
				var cookie = new HttpCookie("auth", FormsAuthentication.Encrypt(ticket));
				Response.Cookies.Set(cookie);
			} else {
				Response.Cookies.Remove("auth");
			}

			return RedirectToAction("Index", "Profile");
		}

		/// <summary>
		/// Форма ввода email для напоминания пароля
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult PasswordRecovery()
		{
			var model = new PasswordUpdate();
			return View(model);
		}

		/// <summary>
		/// Напоминание пароля
		/// </summary>
		/// <param name="model">заполненная модель</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult PasswordRecovery(PasswordUpdate model)
		{
			if (String.IsNullOrEmpty(model.login))
			{
				ErrorMessage("Вы не указали eMail");
				return View(model);
			}

			var user = DB.Account.FirstOrDefault(x => x.Login == model.login && x.TypeUser == (SByte)TypeUsers.ProducerUser);
			// пользователь не найден, отсылаем на домашнюю с ошибкой
			if (user == null)
			{
				ErrorMessage($"Пользователь с email {model.login} не найден, обращайтесь на {ConfigurationManager.AppSettings["MailFrom"]}");
				return View(model);
			}

			// если новый или активный: отсылаем новый пароль на почту
			if (user.EnabledEnum == UserStatus.New || user.EnabledEnum == UserStatus.Active)
			{
				var password = GetRandomPassword();
				user.Password = Md5HashHelper.GetHash(password);
				user.PasswordUpdated = DateTime.Now;
				DB.Entry(user).State = EntityState.Modified;
				DB.SaveChanges();
				Mails.SendPasswordRecoveryMessage(user, password);

				SuccessMessage($"Новый пароль отправлен на ваш email {model.login}");
			}

			// если заблокирован
			else if (user.EnabledEnum == UserStatus.Blocked)
				ErrorMessage($"Ваша учетная запись заблокирована, обращайтесь на {ConfigurationManager.AppSettings["MailFrom"]}");

			// если запросивший регистрацию
			else if (user.EnabledEnum == UserStatus.Request)
				SuccessMessage($"Ваша заявка на регистрацию еще не рассмотрена, обращайтесь на {ConfigurationManager.AppSettings["MailFrom"]}");

			return Redirect("~");
		}

		/// <summary>
		/// Сменить пароль POST
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult ChangePassword(ChangePassword model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = DB.Account.Single(x => x.Id == CurrentUser.Id);
			user.Password = Md5HashHelper.GetHash(model.Pass);
			user.PasswordUpdated = DateTime.Now;
			DB.SaveChanges();

			Mails.SendPasswordChangeMessage(user, model.Pass);
			SuccessMessage("Новый пароль сохранен и отправлен на ваш email: " + user.Login);
			return RedirectToAction("Index", "Profile");
		}

		/// <summary>
		/// Сменить пароль GET
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ChangePassword()
		{
			var model = new ChangePassword();
			return View(model);
		}

		public ActionResult LogOut()
		{
			FormsAuthentication.SignOut();
			Response.Cookies.Remove("auth");
			return Redirect("~");
		}

		/// <summary>
		/// Форма ввода логина-пароля, сейчас только для тестов TODO убрать
		/// </summary>
		/// <returns></returns>
		public ActionResult Auth()
		{
			var model = new LoginValidation();
			return View(model);
		}

		/// <summary>
		/// Возвращает форму выбора производителя
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = new RegProducerViewModel();
			ViewBagProducerList();
			return View(model);
		}

		/// <summary>
		/// Форма регистрации после выбора производителя
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Registration(RegProducerViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBagProducerList();
				return View("Index", model);
			}

			ViewBagAppointmentList(model.ProducerId);
			var producerName = DB.producernames.Single(x => x.ProducerId == model.ProducerId).ProducerName;
			var company = DB.AccountCompany.SingleOrDefault(x => x.ProducerId == model.ProducerId);
			// если от данного производителя регистрировались, возвращаем форму для регистрации пользователя с моделью RegDomainViewModel
			if (company != null)
			{
				var modelDomainView = new RegDomainViewModel() { Producers = model.ProducerId, ProducerName = producerName };
				ViewBag.DomainList = company.CompanyDomainName
					.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() })
					.ToList();
				return View("DomainRegistration", modelDomainView);
			}

			var modelUi = new RegViewModel() { ProducerId = model.ProducerId, ProducerName = producerName };
			return View(modelUi);
		}

		/// <summary>
		/// Регистрация первого пользователя производителя, который включается в группу админов
		/// </summary>
		/// <param name="model">заполненная регистрационная форма</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Registration(RegViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBagAppointmentList(model.ProducerId);
				return View(model);
			}

			// если уже есть такой пользователь
			var user = DB.Account.SingleOrDefault(x => x.Login == model.login);
			if (user != null)
			{
				ErrorMessage("Пользователь с указанным email уже зарегистрирован, попробуйте восстановить пароль");
				ViewBagAppointmentList(model.ProducerId);
				return View(model);
			}

			// если такой компании нет. Компания - это не производитель, это прослойка под производителем
			var company = DB.AccountCompany.SingleOrDefault(x => x.ProducerId == model.ProducerId);
			if (company == null)
			{
				company = new AccountCompany() { ProducerId = model.ProducerId, Name = model.ProducerName };
				DB.AccountCompany.Add(company);
				DB.SaveChanges();

				string domainName = model.login.Split('@')[1].ToLower();
				var domain = new CompanyDomainName() { Name = domainName, CompanyId = company.Id };
				DB.CompanyDomainName.Add(domain);
				DB.SaveChanges();
			}

			// создали новый аккаунт
			var password = GetRandomPassword();
			var account = SaveAccount(accountCompany: company, Reg_ViewModel: model, Pass: password);

			var regionCodes = DB.Regions().Select(x => x.Id).ToList();
			foreach (var regionCode in regionCodes)
				account.AccountRegion.Add(new AccountRegion() { AccountId = account.Id, RegionId = regionCode });

			// добавили аккаунт в группу админов
			var adminGroup = DB.AdminGroup();
			account.AccountGroup.Add(adminGroup);
			account.LastUpdatePermisison = DateTime.Now;
			DB.SaveChanges();

			// отправили письмо о регистрации
			Mails.SendRegistrationMessage(account, password);
			SuccessMessage("Пароль отправлен на ваш email " + account.Login);
			return Redirect("~");
		}

		/// <summary>
		/// Регистрация второго и последующих пользователей производителя
		/// </summary>
		/// <param name="model">заполненная форма регистрации</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DomainRegistration(RegDomainViewModel model)
		{
			var company = DB.AccountCompany.Single(x => x.ProducerId == model.Producers);

			// проверка email
			var domain = company.CompanyDomainName.SingleOrDefault(x => x.Id == model.EmailDomain);
			if (model.Mailname?.Contains("@") == true) {
				var parts = model.Mailname.Split('@');
				model.Mailname = parts[0];
				if (!String.Equals(domain?.Name, parts[1], StringComparison.CurrentCultureIgnoreCase)) {
					ModelState.AddModelError("Mailname", "Домен должен совпадать с выбранным");
				}
			}
			var ea = new EmailAddressAttribute();
			if (domain == null || !ea.IsValid($"{model.Mailname}@{domain.Name}"))
				ModelState.AddModelError("Mailname", "Неверный формат email");

			// если невалидный ввод - возвращаем
			if (!ModelState.IsValid)
			{
				ViewBag.DomainList = company.CompanyDomainName.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() }).ToList();
				ViewBagAppointmentList(model.Producers);
				return View(model);
			}

			// если пользователь с таким email уже регистрировался - возвращаем
			var emailAdress = model.Mailname + "@" + DB.CompanyDomainName.Single(x => x.Id == model.EmailDomain).Name;
			var userExsist = DB.Account.Any(x => x.Login == emailAdress);
			if (userExsist)
			{
				ViewBag.DomainList = company.CompanyDomainName.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() }).ToList();
				ViewBagAppointmentList(model.Producers);
				ErrorMessage("Данный email уже зарегистрирован в нашей базе, попробуйте восстановить пароль");
				return View(model);
			}

			// создали новый аккаунт
			var password = GetRandomPassword();
			var account = SaveAccount(accountCompany: company, RegDomain_ViewModel: model, Pass: password);

			var regionCodes = DB.Regions().Select(x => x.Id).ToList();
			foreach (var regionCode in regionCodes)
				account.AccountRegion.Add(new AccountRegion() { AccountId = account.Id, RegionId = regionCode });

			// ищем группу "все пользователи", если такой нет - создаем
			var otherGroup = DB.AdminGroup();
			// добавляем пользователя в группу все пользователи
			account.AccountGroup.Add(otherGroup);
			account.LastUpdatePermisison = DateTime.Now;
			DB.SaveChanges();

			// отправили письмо о регистрации
			Mails.SendRegistrationMessage(account, password);
			SuccessMessage("Письмо с паролем отправлено на ваш email");
			return Redirect("~");
		}

		/// <summary>
		/// Добавление новой должности
		/// </summary>
		/// <param name="name">имя должности</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddAppointment(string name)
		{
			var appointment = DB.AccountAppointment.FirstOrDefault(x => x.Name == name);
			if (appointment == null)
			{
				appointment = new AccountAppointment() { Name = name, GlobalEnabled = false };
				DB.AccountAppointment.Add(appointment);
				DB.SaveChanges();
			}
			return Json(new {
				id = appointment.Id,
				name = appointment.Name
			});
		}

		/// <summary>
		/// Если компания в списке отсутствует GET
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult CustomRegistration()
		{
			ViewBagAppointmentList();
			var model = new RegNotProducerViewModel();
			return View(model);
		}

		/// <summary>
		/// Если компания в списке отсутствует POST
		/// </summary>
		/// <param name="model">заполненная регистрационная форма</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult CustomRegistration(RegNotProducerViewModel model)
		{
			// если модель невалидна
			if (!ModelState.IsValid)
				return View(model);

			// если такой пользователь уже есть
			var userExist = DB.Account.Any(x => x.Login == model.login);
			if (userExist)
			{
				ErrorMessage("Пользователь с указанным email уже зарегистрирован");
				return View(model);
			}

			// создали и сохранили компанию
			var company = new AccountCompany() { Name = model.CompanyName };
			DB.Entry(company).State = EntityState.Added;
			DB.SaveChanges();

			// создали аккаунт
			// регионы и группы не добавляются здесь, потому что всё равно должен регистрировать админ
			var user = SaveAccount(accountCompany: company, RegNotProducer_ViewModel: model);
			user.AccountRegion = DB.Regions().Select(x => new AccountRegion(user, x)).ToList();
			var group = DB.AdminGroup();
			user.AccountGroup.Add(group);
			user.IP = Request.UserHostAddress;
			DB.SaveChanges();

			// отправили сообщение сотрудникам
			EmailSender.ProducerRequestMessage(DB, user, company.Name, $"{model.PhoneNumber}, {model.login}");
			SuccessMessage("Ваша заявка принята. Ожидайте, с вами свяжутся");
			return Redirect("~");
		}

		/// <summary>
		/// Аутентифицирует администратора панели управления как пользователя
		/// </summary>
		/// <param name="AdminLogin">логин администратора</param>
		/// <param name="IdProducerUSer">идентификатор пользователя</param>
		/// <param name="SecureHash">хеш</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult AdminAuthentification(string AdminLogin, long? IdProducerUSer, string SecureHash)
		{
			if (string.IsNullOrEmpty(AdminLogin)) {
				ErrorMessage("Не указан логин администратора");
				return Redirect("~");
			}

			if (!IdProducerUSer.HasValue) {
				ErrorMessage("Не указан идентификатор пользователя");
				return Redirect("~");
			}

			var adminAccount = DB.Account.SingleOrDefault(x => x.Login == AdminLogin && x.TypeUser == (SByte)TypeUsers.ControlPanelUser);
			if (adminAccount == null)
			{
				ErrorMessage($"Администратор с логином {AdminLogin} не найден");
				return Redirect("~");
			}

			var producerUser = DB.Account.SingleOrDefault(x => x.Id == IdProducerUSer && x.TypeUser == (SByte)TypeUsers.ProducerUser);
			if (producerUser == null)
			{
				ErrorMessage($"Пользователь с идентификатором {IdProducerUSer} не найден");
				return Redirect("~");
			}

			// проверка SecureHash
			string i = "";
			if (adminAccount.Name != null)
				i = (adminAccount.Name.Length * 19801112).ToString();
			else
				i = (18 * 19801112).ToString();
			if (!SecureHash.Contains(i)) {
				ErrorMessage("Неверный SecureHash");
				return Redirect("~");
			}

			if (!adminAccount.SecureTime.HasValue || adminAccount.SecureTime.Value < DateTime.Now) {
				ErrorMessage("Данная ссылка действительна три минуты, время истекло, просьба повторить авторизацию");
				return View("AdminAuth", new AdminAutentification() { IdProducerUser = (long)IdProducerUSer, Login = adminAccount.Login });
			}

			CurrentUser = producerUser;
			Autentificate(adminAccount.Id);
			return RedirectToAction("Index", "Profile");
		}

		/// <summary>
		/// Форма ввода пароля при аутентификации админа как пользователя, если время входа истекло, для повторной авторизации
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AdminAuth(AdminAutentification model)
		{
			var domainAuth = new AdHelper();
			if (domainAuth.IsAuthenticated(model.Login, model.Password))
			{
				// авторизовываем как обычного пользователя, но с добавление ID Администратора
				CurrentUser = DB.Account.Find(model.IdProducerUser);
				var adminId = DB.Account.Single(x => x.Login == model.Login).Id;
				Autentificate(adminId);
			}

			if (CurrentUser != null)
				return RedirectToAction("Index", "Profile");

			model.Password = "";
			ErrorMessage("Пароль указан неверно");
			return View("AdminAuth", model);
		}

		private Account SaveAccount(AccountCompany accountCompany, RegViewModel Reg_ViewModel = null, RegDomainViewModel RegDomain_ViewModel = null, RegNotProducerViewModel RegNotProducer_ViewModel = null, string Pass = null)
		{
			var newAccount = new Account();
			newAccount.EnabledEnum = UserStatus.New;
			newAccount.TypeUser = (sbyte)TypeUsers.ProducerUser;
			newAccount.CompanyId = accountCompany.Id;

			// регистрация первого пользователя компании
			if (Reg_ViewModel != null)
			{
				newAccount.Login = Reg_ViewModel.login;
				newAccount.Password = Md5HashHelper.GetHash(Pass);
				newAccount.PasswordUpdated = DateTime.Now;
				newAccount.FirstName = Reg_ViewModel.FirstName;
				newAccount.LastName = Reg_ViewModel.LastName;
				newAccount.OtherName = Reg_ViewModel.OtherName;
				newAccount.Name = Reg_ViewModel.LastName + " " + Reg_ViewModel.FirstName + " " + Reg_ViewModel.OtherName;
				newAccount.Phone = Reg_ViewModel.PhoneNumber;
				newAccount.AppointmentId = Reg_ViewModel.AppointmentId;
			}

			// регистрация второго и последующих пользователей производителя
			if (RegDomain_ViewModel != null)
			{
				newAccount.Login = RegDomain_ViewModel.Mailname + "@" + DB.CompanyDomainName.Single(x => x.Id == RegDomain_ViewModel.EmailDomain).Name;
				newAccount.Password = Md5HashHelper.GetHash(Pass);
				newAccount.PasswordUpdated = DateTime.Now;
				newAccount.FirstName = RegDomain_ViewModel.FirstName;
				newAccount.LastName = RegDomain_ViewModel.LastName;
				newAccount.OtherName = RegDomain_ViewModel.OtherName;
				newAccount.Name = RegDomain_ViewModel.LastName + " " + RegDomain_ViewModel.FirstName + " " + RegDomain_ViewModel.OtherName;
				newAccount.Phone = RegDomain_ViewModel.PhoneNumber;
				newAccount.AppointmentId = RegDomain_ViewModel.AppointmentId;
			}

			// Если компания в списке отсутствует
			if (RegNotProducer_ViewModel != null)
			{
				// создали новую должность
				var appointment = new AccountAppointment() { Name = RegNotProducer_ViewModel.Appointment, GlobalEnabled = false };
				DB.Entry(appointment).State = EntityState.Added;
				DB.SaveChanges();

				newAccount.Login = RegNotProducer_ViewModel.login;
				newAccount.FirstName = RegNotProducer_ViewModel.FirstName;
				newAccount.LastName = RegNotProducer_ViewModel.LastName;
				newAccount.OtherName = RegNotProducer_ViewModel.OtherName;
				newAccount.Name = RegNotProducer_ViewModel.LastName + " " + RegNotProducer_ViewModel.FirstName + " " + RegNotProducer_ViewModel.OtherName;
				newAccount.Phone = RegNotProducer_ViewModel.PhoneNumber;
				newAccount.AppointmentId = appointment.Id;
				// особый статус
				newAccount.EnabledEnum = UserStatus.Request;
			}

			DB.Entry(newAccount).State = EntityState.Added;
			DB.SaveChanges();
			return newAccount;
		}

		/// <summary>
		/// Кладет в ViewBag.AppointmentList список должностей
		/// </summary>
		/// <param name="ProducerId"></param>
		private void ViewBagAppointmentList(long ProducerId = 0)
		{
			var result = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
			result.AddRange(DB.AccountAppointment.Where(x => x.GlobalEnabled)
					 .ToList()
					 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
					 .ToList());

			// если передан ID производителя, добавляются индивидуальные должности пользователей данного производителя
			if (ProducerId != 0) {
				var appointmentIds = DB.Account.Where(x => x.AccountCompany.ProducerId == ProducerId).Select(x => x.AppointmentId).Distinct().ToList();
				var companyCustomAppointment = DB.AccountAppointment.Where(x => !x.GlobalEnabled && appointmentIds.Contains(x.Id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
				result.AddRange(companyCustomAppointment);
			}
			ViewBag.AppointmentList = result;
		}

		/// <summary>
		/// Кладет в ViewBag.ProducerList список производителей
		/// </summary>
		private void ViewBagProducerList()
		{
			var producerList = new List<OptionElement>() { new OptionElement() { Text = "", Value = "" } };
			producerList.AddRange(
					DB.producernames.Select(x => new OptionElement { Text = x.ProducerName, Value = x.ProducerId.ToString() }).ToList()
			);
			ViewBag.ProducerList = producerList;
		}
	}
}