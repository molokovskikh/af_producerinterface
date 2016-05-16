using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Global;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface.Controllers
{
	public class FeedBackController : MasterBaseController
	{
		/// <summary>
		/// Добавление сообщ обр связи POST
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult SaveFeedBack(FeedBack model)
		{
			if (!ModelState.IsValid)
				return PartialView("FeedBack", model);

			if (string.IsNullOrEmpty(model.PhoneNum) && string.IsNullOrEmpty(model.Email) && CurrentUser == null)
			{
				ViewBag.ErrorMessageModal = "Укажите контакты для связи. Email или номер телефона";
				return PartialView("FeedBack", model);
			}

			var feedBack = new AccountFeedBack();

			if (CurrentUser != null)
			{
				feedBack.Contacts = model.Contact;
				feedBack.AccountId = CurrentUser.Id;
			}
			else
				feedBack.Contacts = model.ContactNotAuth;

			feedBack.Description = model.Description;
			feedBack.UrlString = model.Url;
			feedBack.DateAdd = DateTime.Now;
			feedBack.Type = model.FeedType;
			cntx_.AccountFeedBack.Add(feedBack);
			cntx_.SaveChanges();

			EmailSender.SendFeedBackMessage(cntx_, CurrentUser, feedBack.ToString(), Request.UserHostAddress);

			return PartialView("Success");
		}

		[HttpGet]
		public ActionResult Index()
		{
			ViewBag.TypeMessage = Enum.GetValues(typeof(FeedBackType)).Cast<FeedBackType>();
			return View();
		}

		/// <summary>
		/// Добавление сообщ обр связи, видимо, используется только для запроса доб домена, т.к. должность уже доб сразу без запроса GET
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="IdProducer"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Index_Type(sbyte Id, long IdProducer = 0)
		{
			ViewBag.FBT = (FeedBackTypePrivate)Id;
			ViewBag.TypeMessage = Enum.GetValues(typeof(FeedBackTypePrivate)).Cast<FeedBackTypePrivate>();

			var model = new FeedBack();

			if (Id == (sbyte)FeedBackTypePrivate.AddNewAppointment)
				model.Description = "Просьба добавить мою должность: ";
			else if (Id == (sbyte)FeedBackTypePrivate.AddNewDomainName)
			{
				var producerName = cntx_.producernames.First(x => x.ProducerId == IdProducer).ProducerName;
				ViewBag.ProducerName = producerName;
				model.Description = $"Я являюсь сотрудником компании {producerName}, не могу зарегистрироваться в связи с отсутствием домена моего почтового ящика, прошу добавить возможность регистрации с моим email";
				return View("FeedBackNewDomain", model);
			}

			return View("Index", model);
		}

		/// <summary>
		/// Добавление сообщ обр связи POST. Видимо, не используется
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Index(FeedBack model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.TypeMessage = Enum.GetValues(typeof(FeedBackType)).Cast<FeedBackType>();
				ViewBag.FBT = (FeedBackType)model.FeedType;
				return View(model);
			}

			if (CurrentUser == null && string.IsNullOrEmpty(model.Email) && model.FeedType == (sbyte)FeedBackTypePrivate.AddNewDomainName)
			{
				ViewBag.FBT = (FeedBackTypePrivate)model.FeedType;
				ViewBag.TypeMessage = Enum.GetValues(typeof(FeedBackTypePrivate)).Cast<FeedBackTypePrivate>();
				ViewBag.ErrorMessageModal = "поле Email является обязательным для заполнения";
				return View("Index", model);
			}

			var feedBack = new AccountFeedBack();

			if (model.FeedType == (sbyte)FeedBackTypePrivate.AddNewAppointment)
				feedBack.UrlString = "Обратная взязь в ЛК";
			else if (model.FeedType == (sbyte)FeedBackTypePrivate.AddNewDomainName)
				feedBack.UrlString = "Регистрация нового пользователя для зарегистированного производителя";
			if (string.IsNullOrEmpty(feedBack.UrlString))
				feedBack.UrlString = "~/FeedBack/Index_Type/";
			if (CurrentUser != null)
				feedBack.AccountId = CurrentUser.Id;

			feedBack.Description = model.Description;
			feedBack.Type = model.FeedType;
			feedBack.Contacts = model.Contact;
			feedBack.DateAdd = DateTime.Now;
			cntx_.AccountFeedBack.Add(feedBack);
			cntx_.SaveChanges();

			SuccessMessage("Выша заявка принята к исполнению");
			return RedirectToAction("Index", "Profile");
		}

		/// <summary>
		/// Запрос о добавлении домена в обр связь, видимо, только POST
		/// </summary>
		/// <param name="FIO"></param>
		/// <param name="Email"></param>
		/// <param name="PhoneNum"></param>
		/// <param name="CompanyNames"></param>
		/// <returns></returns>
		public ActionResult FeedBakcAddNewDomain(string FIO, string Email, string PhoneNum, string CompanyNames)
		{

			var feedBack = new AccountFeedBack();

			feedBack.Contacts = $"{PhoneNum}, {Email}";
			feedBack.Type = (sbyte)FeedBackTypePrivate.AddNewDomainName;
			feedBack.Status = (sbyte)FeedBackStatus.New;
			feedBack.UrlString = $"Заявка подана при невозможности зарегистрироватся с другим доменом для производителя {CompanyNames}";
			feedBack.Description = $"Я, {FIO}, являюсь сотрудником компании {CompanyNames}, использую в своей деятельности email {Email}, но система не позволяет мне зарегистрироваться с этим email. Прошу решить возникшую  проблему. Телефон для связи {PhoneNum}";
			feedBack.DateAdd = DateTime.Now;

			cntx_.AccountFeedBack.Add(feedBack);
			cntx_.SaveChanges();

			SuccessMessage("Выша заявка принята к исполнению");
			return RedirectToAction("index", "home");
		}

		/// <summary>
		/// Вызывается при закрытии формы обратной связи для её очистки ??
		/// </summary>
		/// <returns></returns>
		public ActionResult GetView()
		{
			var model = new FeedBack();
			return PartialView("FeedBack", model);
		}
	}
}
