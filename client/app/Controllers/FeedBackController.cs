using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ViewModel.Interface.Global;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterface.Controllers
{
	public class FeedBackController : BaseController
	{
		/// <summary>
		/// Добавление сообщ обр связи при клике на "Что-то не так"
		/// </summary>
		/// <param name="model">заполненная модель сообщения</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult SaveFeedBack(FeedBack model)
		{
			if (!ModelState.IsValid)
				return PartialView("FeedBack", model);

			if (string.IsNullOrEmpty(model.PhoneNum) && string.IsNullOrEmpty(model.Email) && CurrentUser == null)
			{
				ModelState.AddModelError("Email", "Укажите контакты для связи. Email или номер телефона");
				ModelState.AddModelError("PhoneNum", "Укажите контакты для связи. Email или номер телефона");
				return PartialView("FeedBack", model);
			}

			var feedBack = new AccountFeedBack()
			{
				Description = model.Description,
				UrlString = model.Url,
				DateAdd = DateTime.Now,
				Type = model.FeedType
			};

			if (CurrentUser != null)
			{
				feedBack.Contacts = model.Contact;
				feedBack.AccountId = CurrentUser.Id;
			}
			else
				feedBack.Contacts = model.ContactNotAuth;

			DB.AccountFeedBack.Add(feedBack);
			DB.SaveChanges();

			EmailSender.SendFeedBackMessage(DB, CurrentUser, feedBack.ToString(), Request.UserHostAddress);
			return PartialView("Success");
		}

		/// <summary>
		/// Добавление сообщ обр связи для запроса должности из профиля
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult AddAppointmentRequest()
		{
			if(CurrentUser == null)
				throw new NotSupportedException("Добавление должности не из профиля");

			var model = new FeedBack() {
				Email = CurrentUser.Login,
				PhoneNum = CurrentUser.Phone,
				Description = "Просьба добавить мою должность: "
			};
			return View(model);
		}

		/// <summary>
		/// Добавление сообщ обр связи для запроса должности из профиля
		/// </summary>
		/// <param name="model">заполненная модель сообщения</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddAppointmentRequest(FeedBack model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var feedBack = new AccountFeedBack()
			{
				Description = model.Description,
				Type = (sbyte)FeedBackTypePrivate.AddNewAppointment,
				Contacts = model.Contact,
				DateAdd = DateTime.Now,
				UrlString = "Добавление должности",
				AccountId = CurrentUser.Id
			};

			DB.AccountFeedBack.Add(feedBack);
			DB.SaveChanges();

			EmailSender.SendFeedBackMessage(DB, CurrentUser, feedBack.ToString(), Request.UserHostAddress);
			SuccessMessage("Ваша заявка принята к исполнению");
			return RedirectToAction("Index", "Profile");
		}

		/// <summary>
		/// Добавление сообщ обр связи для запроса доб домена из регистрации
		/// </summary>
		/// <param name="producerId">идентификатор производителя</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult AddDomainRequest(long? producerId)
		{
			var producer = DB.producernames.SingleOrDefault(x => x.ProducerId == producerId);
			if (producer == null)
				throw new NotSupportedException("Производитель не найден");

			var model = new AddDomainFeedBack() {
				ProducerName = producer.ProducerName
			};
			return View(model);
		}

		/// <summary>
		/// Добавление сообщ обр связи для запроса доб домена из регистрации
		/// </summary>
		/// <param name="model">заполненная модель сообщения</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult AddDomainRequest(AddDomainFeedBack model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var feedBack = new AccountFeedBack() {
				Contacts = model.Contact,
				Type = (sbyte)FeedBackTypePrivate.AddNewDomainName,
				UrlString = $"Добавление домена для производителя {model.ProducerName}",
				Description = $"{model.PresetDescription} {model.Description}",
				DateAdd = DateTime.Now
			};

			DB.AccountFeedBack.Add(feedBack);
			DB.SaveChanges();

			EmailSender.SendFeedBackMessage(DB, CurrentUser, feedBack.ToString(), Request.UserHostAddress);
			SuccessMessage("Ваша заявка принята к исполнению");
			return Redirect("~");
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
