using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.GlobalAccount;
using ProducerInterfaceCommon.Heap;
using System.Data.Entity;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class GlobalAccountController : MasterBaseController
	{

		sbyte Type = (sbyte)TypeUsers.ProducerUser;

		/// <summary>
		/// Список пользователей без производителя GET: GlobalAccount
		/// </summary>
		/// <returns></returns>
		public ActionResult Index(bool active = false)
		{
			var model = cntx_.Account.Where(x => x.TypeUser == Type && !x.AccountCompany.ProducerId.HasValue).ToList();
			// item.Enabled == 0 && item.PasswordUpdated.HasValue
			return View(model);
		}

		//[HttpGet]
		//public ActionResult DeleteAccount(long id)
		//{
		//	SuccessMessage("Пока не реализовано");
		//	return RedirectToAction("Index");
		//}

		/// <summary>
		/// Подтверждение регистрации пользователя без производителя GET
		/// </summary>
		/// <param name="id">идентификатор пользователя</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult AccountVerification(long id)
		{
			var AccountModel = cntx_.Account.Find(id);

			var model = cntx_.Account.Where(x => x.Id == id).ToList().Select(x => new RegistrationCustomProfile
			{
				Id = x.Id,
				AppointmentId = (int)x.AppointmentId,
				CompanyName = x.AccountCompany.Name,
				FirstName = x.FirstName,
				LastName = x.LastName,
				Login = x.Login,
				OtherName = x.OtherName,
				Phone = x.Phone,
				SelectedGroupList = new List<int>(),
				CompanyId = x.AccountCompany.Id
			}
			).First();

			// список должностей общий для всех + для данного пользователя
			ViewBag.PostList = cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1 || x.Account.Any(z => z.Id == id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
			// список групп пользователей
			ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.TypeGroup == (sbyte)TypeUsers.ProducerUser && x.Enabled).ToList().Select(x => new OptionElement
			{
				Text = x.Name + " " + x.Description,
				Value = x.Id.ToString()
			}).ToList();

			return View(model);

		}

		/// <summary>
		/// Подтверждение регистрации пользователя без производителя POST
		/// </summary>
		/// <param name="model">заполненная регистрационная форма</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AccountVerification(RegistrationCustomProfile model)
		{
			if (!ModelState.IsValid)
			{
				// список должностей общий для всех + для данного пользователя
				ViewBag.PostList = cntx_.AccountAppointment.Where(x => x.GlobalEnabled == 1 || x.Account.Any(z => z.Id == model.Id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
				// список групп пользователей
				ViewBag.GroupList = cntx_.AccountGroup.Where(x => x.TypeGroup == (sbyte)TypeUsers.ProducerUser && x.Enabled).ToList().Select(x => new OptionElement
				{
					Text = x.Name + " " + x.Description,
					Value = x.Id.ToString()
				}).ToList();

				return View(model);
			}

			string password = "";
			var user = SaveAccount(model, ref password);

			// отправка сообщения пользователю с паролем.
			EmailSender.SendAccountVerificationMessage(cntx_, user.Id, password, CurrentUser.IP, CurrentUser.Id);
			SuccessMessage("Регистрация пользователя подтверждена");
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Сохраняет изменения в пользователе и компании
		/// </summary>
		/// <param name="model">заполненная форма регистрации</param>
		/// <param name="password">возвращает пароль</param>
		/// <returns></returns>
		private Account SaveAccount(RegistrationCustomProfile model, ref string password)
		{
			var user = cntx_.Account.Find(model.Id);

			// разблокировали пользователя
			password = GetRandomPassword();
			user.Password = Md5HashHelper.GetHash(password);
			user.PasswordUpdated = DateTime.Now;
			user.Login = model.Login;
			user.Name = model.LastName + " " + model.FirstName + " " + model.OtherName;
			user.LastName = model.LastName;
			user.FirstName = model.FirstName;
			user.OtherName = model.OtherName;
			user.Phone = model.Phone;
			user.UserType = (sbyte)TypeUsers.ProducerUser;
			user.AppointmentId = model.AppointmentId;
			user.EnabledEnum = UserStatus.New;

			cntx_.Entry(user).State = EntityState.Modified;
			cntx_.SaveChanges();

			foreach (var item in model.SelectedGroupList)
				user.AccountGroup.Add(cntx_.AccountGroup.Find(item));

			cntx_.Entry(user).State = EntityState.Modified;

			var accountCompany = cntx_.AccountCompany.Find(model.CompanyId);
			if (string.IsNullOrEmpty(model.CompanyName))
				accountCompany.Name = "Физическое лицо";
			else
				accountCompany.Name = model.CompanyName;
			cntx_.Entry(accountCompany).State = EntityState.Modified;

			cntx_.SaveChanges();

			return user;
		}
	}
}