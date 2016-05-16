using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Global
{
	public class FeedBack
	{
		[Required(ErrorMessage = "Заполните сообщение")]
		[Display(Name = "Сообщение")]
		public string Description { get; set; }

		[Display(Name = "Выберите способ для связи")]
		public string Contact { get; set; }

		[Display(Name = "Номер телефона")]
		[Phone(ErrorMessage = "Некорректно введен номер")]
		[StringLength(maximumLength: 15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
		public string PhoneNum { get; set; }


		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Введите корректый email")]
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
		public sbyte FeedType { get; set; }
	}
}
