using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Global
{
	public class FeedBack
	{
		[Required(ErrorMessage = "Заполните сообщение")]
		[Display(Name = "Сообщение")]
		[StringLength(500, ErrorMessage = "Длина сообщения не более 500 знаков")]
		public string Description { get; set; }

		[Display(Name = "Выберите удобный способ для связи с вами")]
		public string Contact { get; set; }

		[Display(Name = "Номер телефона")]
		[Phone(ErrorMessage = "Некорректно введен номер")]
		[StringLength(maximumLength: 15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
		public string PhoneNum { get; set; }


		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Введите корректый email")]
		[StringLength(100, ErrorMessage = "Длина сообщения не более 100 знаков")]
		public string Email { get; set; }

		public string ContactNotAuth
		{
			get
			{
				if (!string.IsNullOrEmpty(PhoneNum) && !string.IsNullOrEmpty(Email))
				{
					return PhoneNum + " " + Email;
				}
				else
				{
					if (string.IsNullOrEmpty(PhoneNum))
					{ return Email; }
					else
					{ return PhoneNum; }
				}
			}
		}

		public string Url { get; set; }

		[Display(Name = "Тип обращения")]
		public sbyte FeedType { get; set; }
	}

	public class AddDomainFeedBack
	{
		public string PresetDescription
		{
			get { return $"Я, {Name}, являюсь сотрудником компании {ProducerName}, не могу зарегистрироваться в связи с отсутствием домена моего почтового ящика, прошу добавить возможность регистрации с моим email."; }
		}

		[Display(Name = "Дополнительное сообщение")]
		[StringLength(200, ErrorMessage = "Длина сообщения не более 200 знаков")]
		public string Description { get; set; }

		[Display(Name = "Номер телефона *")]
		[Phone(ErrorMessage = "Некорректно введен номер")]
		[StringLength(maximumLength: 15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
		[Required(ErrorMessage = "Укажите номер телефона")]
		public string PhoneNum { get; set; }

		[Display(Name = "Email *")]
		[EmailAddress(ErrorMessage = "Введите корректый email")]
		[StringLength(100, ErrorMessage = "Длина email не более 100 знаков")]
		[Required(ErrorMessage = "Укажите email")]
		public string Email { get; set; }

		[Display(Name = "ФИО *")]
		[Required(ErrorMessage = "Укажите ФИО")]
		[StringLength(135, ErrorMessage = "Длина ФИО не более 135 знаков")]
		public string Name { get; set; }

		public string Contact
		{
			get { return $"{PhoneNum}, {Email}"; }
		}

		public string ProducerName { get; set; }

	}
}
