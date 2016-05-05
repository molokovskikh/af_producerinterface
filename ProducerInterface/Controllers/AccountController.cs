using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Registration;
using ProducerInterfaceCommon.Controllers;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface.Controllers
{
	public class AccountController : AccountAuthController
	{

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
			var producerName = cntx_.producernames.Single(x => x.ProducerId == model.ProducerId).ProducerName;
			var companyExsist = cntx_.AccountCompany.Any(x => x.ProducerId == model.ProducerId);
			// если от данного производителя регистрировались, возвращаем форму для регистрации пользователя с моделью RegDomainViewModel
			if (companyExsist)
			{
				var modelDomainView = new RegDomainViewModel() { Producers = model.ProducerId, ProducerName = producerName };
				ViewBag.DomainList = cntx_.AccountCompany
					.First(x => x.ProducerId == model.ProducerId).CompanyDomainName
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
			var userExsist = cntx_.Account.Any(x => x.Login == model.login);
			if (userExsist)
			{
				ErrorMessage("Пользователь с указанным email уже зарегистрирован, попробуйте восстановить пароль");
				ViewBagAppointmentList(model.ProducerId);
				return View(model);
			}

			// если такой компании нет. Компания - это не производитель, это прослойка под производителем
			var company = cntx_.AccountCompany.FirstOrDefault(x => x.ProducerId == model.ProducerId);
			if (company == null)
			{
				company = new AccountCompany() { ProducerId = model.ProducerId, Name = model.ProducerName };
				cntx_.Entry(company).State = EntityState.Added;
				cntx_.SaveChanges();

				string domainName = model.login.Split('@')[1].ToLower();
				var domain = new CompanyDomainName() { Name = domainName, CompanyId = company.Id };
				cntx_.Entry(domain).State = EntityState.Added;
				cntx_.SaveChanges();

				company.CompanyDomainName.Add(domain);
				cntx_.SaveChanges();
			}

			// создали новый аккаунт
			var password = GetRandomPassword();
			var account = SaveAccount(accountCompany: company, Reg_ViewModel: model, Pass: password);

			// добавили аккаунт в группу админов
			var adminGroupName = GetWebConfigParameters("AdminGroupName");
			var adminGroup = cntx_.AccountGroup.SingleOrDefault(x => x.Name == adminGroupName && x.TypeGroup == SbyteTypeUser);
			if (adminGroup == null)
			{
				adminGroup = new AccountGroup { Name = adminGroupName, Enabled = true, Description = "Администраторы", TypeGroup = SbyteTypeUser };
				cntx_.AccountGroup.Add(adminGroup);
				cntx_.SaveChanges();
			}

			account.AccountGroup.Add(adminGroup);
			account.LastUpdatePermisison = DateTime.Now;
			cntx_.SaveChanges();

			// отправили письмо о регистрации
			EmailSender.SendRegistrationMessage(cntx_, account.Id, password, account.IP);
			SuccessMessage("Пароль отправлен на ваш email " + account.Login);
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// Регистрация второго и последующих пользователей производителя
		/// </summary>
		/// <param name="model">заполненная форма регистрации</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult DomainRegistration(RegDomainViewModel model)
		{
			var company = cntx_.AccountCompany.Single(x => x.ProducerId == model.Producers);
			// если невалидный ввод - возвращаем
			if (!ModelState.IsValid)
			{
				ViewBag.DomainList = company.CompanyDomainName.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() }).ToList();
				ViewBagAppointmentList(model.Producers);
				return View(model);
			}

			// если пользователь с таким email уже регистрировался - возвращаем
			var emailAdress = model.Mailname + "@" + cntx_.CompanyDomainName.Single(x => x.Id == model.EmailDomain).Name;
			var userExsist = cntx_.Account.Any(x => x.Login == emailAdress);
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

			// ищем группу "все пользователи", если такой нет - создаем
			var otherGroupName = GetWebConfigParameters("LogonGroupAcess");
			var otherGroup = cntx_.AccountGroup.FirstOrDefault(x => x.Name == otherGroupName && x.TypeGroup == account.TypeUser);
			if (otherGroup == null)
			{
				otherGroup = new AccountGroup() { Name = otherGroupName, Description = "вновь зарегистрированные пользователи", Enabled = true };
				cntx_.Entry(otherGroup).State = EntityState.Added;
				cntx_.SaveChanges();

				// добавляем в группу права TODO уточнить, какие
				var permissionList = cntx_.AccountPermission.Where(x => x.Enabled && x.TypePermission == account.TypeUser).ToList();
				foreach (var item in permissionList)
					otherGroup.AccountPermission.Add(item);

				cntx_.Entry(otherGroup).State = EntityState.Modified;
				cntx_.SaveChanges();
			}

			// добавляем пользователя в группу все пользователи
			account.AccountGroup.Add(otherGroup);
			account.LastUpdatePermisison = DateTime.Now;
			cntx_.SaveChanges();

			// отправили письмо о регистрации
			EmailSender.SendRegistrationMessage(cntx_, account.Id, password, HttpContext.Request.UserHostAddress);
			SuccessMessage("Письмо с паролем отправлено на ваш email");
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult DolznostAddNew(string NewNameAppointment)
		{
			var NewAppExsist = cntx_.AccountAppointment.Any(xxx => xxx.Name == NewNameAppointment);
			var NewAppointment_ = new AccountAppointment();

			if (NewAppExsist)
			{
				NewAppointment_ = cntx_.AccountAppointment.Where(xxx => xxx.Name == NewNameAppointment).First();
			}
			else
			{
				NewAppointment_.Name = NewNameAppointment;
				NewAppointment_.GlobalEnabled = 0;
				cntx_.Entry(NewAppointment_).State = EntityState.Added;
				cntx_.SaveChanges();
			}

			return Content(NewAppointment_.Id.ToString() + ";" + NewAppointment_.Name);
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
			var userExist = cntx_.Account.Any(x => x.Login == model.login);
			if (userExist)
			{
				ErrorMessage("Пользователь с указанным email уже зарегистрирован");
				return View(model);
			}

			// создали и сохранили компанию
			var company = new AccountCompany() { Name = model.CompanyName };
			cntx_.Entry(company).State = EntityState.Added;
			cntx_.SaveChanges();

			// создали аккаунт
			var user = SaveAccount(accountCompany: company, RegNotProducer_ViewModel: model);

			// отправили сообщение в обратную связь
			var feedBack = new AccountFeedBack() {
				Contacts = $"{model.PhoneNumber}, {model.login}",
				Description = $"Запрос на регистрацию, компания {company.Name} (id={company.Id})",
				DateAdd = DateTime.Now,
				AccountId = user.Id,
				UrlString = "~/Regisration/CustomRegistration",
				Type = (sbyte)FeedBackTypePrivate.Registration
			};
			cntx_.AccountFeedBack.Add(feedBack);
			cntx_.SaveChanges();

			EmailSender.ProducerRequestMessage(cntx_, user, feedBack);
			SuccessMessage("Ваша заявка принята. Ожидайте, с вами свяжутся");
			return RedirectToAction("Index", "Home");
		}


		[HttpGet]
		public ActionResult AdminAuthentification(string AdminLogin, long? IdProducerUSer, string SecureHash)
		{
			if (AdminLogin == null || IdProducerUSer == null)
				return RedirectToAction("index", "home");

			// проверка наличия Админа В БД.

			var AccountAdminExsist = cntx_.Account.Any(xxx => xxx.Login == AdminLogin && xxx.TypeUser == (SByte)TypeUsers.ControlPanelUser);
			var ProducerUserExsist = cntx_.Account.Any(xxx => xxx.Id == IdProducerUSer && xxx.TypeUser == (SByte)TypeUsers.ProducerUser);

			// проверка SecureHash
			var adminAccount = cntx_.Account.Single(xxx => xxx.Login == AdminLogin);

			string i = "";
			if (adminAccount.Name != null)
				i = (adminAccount.Name.Length * 19801112).ToString();
			else
				i = (18 * 19801112).ToString();

			if (!SecureHash.Contains(i))
				return RedirectToAction("index", "home");

			if (adminAccount.SecureTime.Value > DateTime.Now)
			{
				CurrentUser = cntx_.Account.Find(IdProducerUSer);
				var AdminId = adminAccount.Id.ToString();
				Autentificate(AdminId);
				return RedirectToAction("Index", "Profile");
			}
			else
			{
				ErrorMessage("Данная ссылка действительна три минуты, время истекло, просьба повторить авторизацию");
				return View("AdminAuth", new AdminAutentification() { IdProducerUser = (long)IdProducerUSer, Login = adminAccount.Login });
			}
		}

		[HttpPost]
		public ActionResult AdminAuth(AdminAutentification model)
		{
			var DomainAuth = new DomainAutentification();
			if (DomainAuth.IsAuthenticated(model.Login, model.Password))
			{
				// авторизовываем как обычного пользователя, но с добавление ID Администратора 
				CurrentUser = cntx_.Account.Find(model.IdProducerUser);
				var AdminId = cntx_.Account.Single(x => x.Login == model.Login).Id.ToString();
				Autentificate(AdminId);
			}

			if (CurrentUser != null)
				return RedirectToAction("Index", "Profile");

			model.Password = "";
			ErrorMessage("Пароль указан не верно");
			return View("AdminAuth", model);
		}

		private Account SaveAccount(AccountCompany accountCompany, RegViewModel Reg_ViewModel = null, RegDomainViewModel RegDomain_ViewModel = null, RegNotProducerViewModel RegNotProducer_ViewModel = null, string Pass = null)
		{
			var newAccount = new Account();
			newAccount.EnabledEnum = UserStatus.New;
			newAccount.RegionMask = 0;
			newAccount.TypeUser = (sbyte)TypeUsers.ProducerUser;
			newAccount.CompanyId = accountCompany.Id;

			// регистрация первого пользователя компании
			if (Reg_ViewModel != null)
			{
				newAccount.Login = Reg_ViewModel.login;
				newAccount.Password = Md5HashHelper.GetHash(Pass);
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
				newAccount.Login = RegDomain_ViewModel.Mailname + "@" + cntx_.CompanyDomainName.Single(x => x.Id == RegDomain_ViewModel.EmailDomain).Name;
				newAccount.Password = Md5HashHelper.GetHash(Pass);
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
				var appointment = new AccountAppointment() { Name = RegNotProducer_ViewModel.Appointment, GlobalEnabled = 0 };
				cntx_.Entry(appointment).State = EntityState.Added;
				cntx_.SaveChanges();

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

			cntx_.Entry(newAccount).State = EntityState.Added;
			cntx_.SaveChanges();
			return newAccount;
		}

		/// <summary>
		/// Кладет в ViewBag.AppointmentList список должностей
		/// </summary>
		/// <param name="ProducerId"></param>
		private void ViewBagAppointmentList(long ProducerId = 0)
		{
			var result = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
			result.AddRange(cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1 && x.IdAccount == null)
					 .ToList()
					 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
					 .ToList());

			// если передан ID производителя, добавляются должности данного производителя
			if (ProducerId != 0)
			{
				var companyCustomAppointment = cntx_.AccountAppointment.Where(x => x.Account1.AccountCompany.ProducerId == ProducerId).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
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
					cntx_.producernames.Select(x => new OptionElement { Text = x.ProducerName, Value = x.ProducerId.ToString() }).ToList()
			);
			ViewBag.ProducerList = producerList;
		}
	}
}