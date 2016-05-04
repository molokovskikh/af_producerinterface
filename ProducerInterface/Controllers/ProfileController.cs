using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.Interface.Profile;

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

			var model = new ProfileValidation();
			model.AppointmentId = thisUser.AccountAppointment.Id;
			model.CompanyName = cntx_.producernames.Single(x => x.ProducerId == thisUser.AccountCompany.ProducerId).ProducerName;
			model.EmailDomain = thisUser.AccountCompany.CompanyDomainName.First(x => thisUser.Login.Contains(x.Name)).Id;
			model.Mailname = thisUser.Login.Split('@')[0];
			model.PhoneNumber = thisUser.Phone;
			model.LastName = thisUser.LastName;
			model.FirstName = thisUser.FirstName;
			model.OtherName = thisUser.OtherName;

			var appointmentList =
			 cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
					 .ToList()
					 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
					 .ToList();

			var userOptionAppointment = cntx_.AccountAppointment.Where(x => x.Id == thisUser.AccountAppointment.Id).ToList().Select(x => new OptionElement { Value = x.Id.ToString(), Text = x.Name }).First();
			if (!appointmentList.Contains(userOptionAppointment))
				appointmentList.Add(userOptionAppointment);

			ViewBag.AppointmentList = appointmentList;
			ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

			return View(model);
		}

		[HttpPost]
		public ActionResult Account(ProfileValidation changeProfile)
		{

			if (!ModelState.IsValid)
			{

				var AppointmentList =
				 cntx_.AccountAppointment.Where(xx => xx.GlobalEnabled == 1)
						 .ToList()
						 .Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() })
						 .ToList();

				var UserOptionAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == CurrentUser.AccountAppointment.Id).ToList().Select(xxx => new OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name }).First();

				if (!AppointmentList.Contains(UserOptionAppointment))
				{
					AppointmentList.Add(UserOptionAppointment);
				}

				ViewBag.AppointmentList = AppointmentList;
				ViewBag.DomainList = cntx_.AccountCompany.Where(xxx => xxx.ProducerId == CurrentUser.AccountCompany.ProducerId).First().CompanyDomainName.Select(xxx => new OptionElement { Text = '@' + xxx.Name, Value = xxx.Id.ToString() }).ToList();

				return View(changeProfile);
			}

			var AccountSave = cntx_.Account.Where(xxx => xxx.Id == CurrentUser.Id).First();

			AccountSave.Name = changeProfile.LastName + " " + changeProfile.FirstName + " " + changeProfile.OtherName;
			AccountSave.LastName = changeProfile.LastName;
			AccountSave.FirstName = changeProfile.FirstName;
			AccountSave.OtherName = changeProfile.OtherName;

			AccountSave.Login = changeProfile.Mailname + "@" + AccountSave.AccountCompany.CompanyDomainName.Where(xxx => xxx.Id == changeProfile.EmailDomain).First().Name;
			AccountSave.Phone = changeProfile.PhoneNumber;
			AccountSave.AccountAppointment = cntx_.AccountAppointment.Where(xxx => xxx.Id == changeProfile.AppointmentId).First();

			cntx_.Entry(AccountSave).State = System.Data.Entity.EntityState.Modified;
			cntx_.SaveChanges();

			SuccessMessage("Ваш профиль сохранен");
			return RedirectToAction("Index");
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

