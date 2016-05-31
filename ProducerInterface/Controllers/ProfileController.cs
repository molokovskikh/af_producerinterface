using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.Interface.Profile;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterface.Controllers
{
	public class ProfileController : MasterBaseController
	{

		private int PagerCount = 5;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ViewBag.BreadCrumb = "Профиль пользователя";
		}

		public ActionResult Index()
		{
			ViewBag.Pager = 1;

			var newsAll = cntx_.NotificationToProducers.Where(x => x.Enabled).ToList();
			newsAll.Reverse();

			ViewBag.News = newsAll.OrderByDescending(x => x.DatePublication).Take(PagerCount).ToList();
			ViewBag.MaxCount = newsAll.Count();
			return View();
		}

		/// <summary>
		/// Мой профиль GET
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Account()
		{

			var thisUser = cntx_.Account.Single(x => x.Id == CurrentUser.Id);

			var model = new ProfileValidation() {
				AppointmentId = thisUser.AppointmentId,
				CompanyName = thisUser.AccountCompany.Name,
				EmailDomain = thisUser.AccountCompany.CompanyDomainName.Single(x => x.Name == thisUser.Login.Split('@')[1]).Id,
				Mailname = thisUser.Login.Split('@')[0],
				PhoneNumber = thisUser.Phone,
				LastName = thisUser.LastName,
				FirstName = thisUser.FirstName,
				OtherName = thisUser.OtherName
			};

			var appointmentList =
			 cntx_.AccountAppointment.Where(x => x.GlobalEnabled)
					 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
					 .ToList();

			// добавили кастомную должность, если есть
			var userOptionAppointment = cntx_.AccountAppointment.SingleOrDefault(x => x.Id == CurrentUser.AppointmentId && !x.GlobalEnabled);
			if (userOptionAppointment != null)
				appointmentList.Add(new OptionElement { Value = userOptionAppointment.Id.ToString(), Text = userOptionAppointment.Name });

			ViewBag.AppointmentList = appointmentList;
			ViewBag.DomainList = cntx_.CompanyDomainName
				.Where(x => x.CompanyId == CurrentUser.CompanyId)
				.ToList()
				.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() })
				.ToList();

			return View(model);
		}

		[HttpPost]
		public ActionResult Account(ProfileValidation model)
		{
			var domain = cntx_.CompanyDomainName.Single(x => x.Id == model.EmailDomain).Name;
			var newLogin = $"{model.Mailname}@{domain}";

			var ea = new EmailAddressAttribute();
			if (!ea.IsValid(newLogin))
				ModelState.AddModelError("Mailname", "Неверный формат email");

			if (!ModelState.IsValid)
			{
				var appointmentList =
				 cntx_.AccountAppointment.Where(x => x.GlobalEnabled)
						 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
						 .ToList();

				// добавили кастомную должность, если есть
				var userOptionAppointment = cntx_.AccountAppointment.SingleOrDefault(x => x.Id == CurrentUser.AppointmentId && !x.GlobalEnabled);
				if (userOptionAppointment != null)
					appointmentList.Add(new OptionElement { Value = userOptionAppointment.Id.ToString(), Text = userOptionAppointment.Name });

				ViewBag.AppointmentList = appointmentList;
				ViewBag.DomainList = cntx_.CompanyDomainName
					.Where(x => x.CompanyId == CurrentUser.CompanyId)
					.ToList()
					.Select(x => new OptionElement { Text = '@' + x.Name, Value = x.Id.ToString() })
					.ToList();

				return View(model);
			}


			var accountSave = cntx_.Account.Single(x => x.Id == CurrentUser.Id);
			var changeLogin = accountSave.Login != newLogin;

			accountSave.Name = $"{model.LastName} {model.FirstName} {model.OtherName}";
			accountSave.LastName = model.LastName;
			accountSave.FirstName = model.FirstName;
			accountSave.OtherName = model.OtherName;
			accountSave.Login = newLogin;
			accountSave.Phone = model.PhoneNumber;
			accountSave.AppointmentId = model.AppointmentId;
			cntx_.SaveChanges();

			if (changeLogin) {
				CurrentUser = accountSave;
				SetUserCookiesName(CurrentUser.Login, true, "");
				SetCookie("AccountName", CurrentUser.Login, false);
			}

			SuccessMessage("Ваш профиль сохранен");
			return RedirectToAction("Index", "Profile");
		}

		public ActionResult GetOldNews(int Pages)
		{
			var News = cntx_.NotificationToProducers.OrderByDescending(x => x.DatePublication).Skip(Pages * 10).Take(10).ToList();
			return PartialView(News);
		}

		public ActionResult GetNextList(int Pager)
		{
			ViewBag.Pager = Pager + 1;
			var ListNews10 = cntx_.NotificationToProducers.OrderByDescending(xxx => xxx.DatePublication).ToList().Skip(PagerCount * Pager).Take(PagerCount).ToList();

			ViewBag.MaxCount = cntx_.NotificationToProducers.Count() / (PagerCount * Pager);
			return PartialView("GetNextList", ListNews10);
		}

		public ActionResult News(int Id)
		{
			var News = cntx_.NotificationToProducers.Where(xxx => xxx.Id == Id).First();
			return View(News);
		}

	}
}

