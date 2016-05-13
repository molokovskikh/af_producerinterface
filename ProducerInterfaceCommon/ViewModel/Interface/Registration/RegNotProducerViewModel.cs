using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Registration
{
	/* Модель для регистрации независимого наблюдателя (назовом его Физ. Лицом) */
	public class RegNotProducerViewModel
	{
		[UIHint("EditorString")]
		[Display(Name = "Фамилия")]
		[MaxLength(30, ErrorMessage = "Максимальная длина 30 знаков")]
		[Required(ErrorMessage = "Введите фамилию")]
		public string LastName { get; set; }

		[UIHint("EditorString")]
		[Display(Name = "Имя")]
		[MaxLength(30, ErrorMessage = "Максимальная длина 30 знаков")]
		[Required(ErrorMessage = "Введите Имя")]
		public string FirstName { get; set; }

		[UIHint("EditorString")]
		[Display(Name = "Отчество")]
		[MaxLength(30, ErrorMessage = "Максимальная длина 30 знаков")]
		public string OtherName { get; set; }

		// TODO почему так мало знаков
		[UIHint("EditorString")]
		[Display(Name = "Название компании")]
		[Required(ErrorMessage = "Укажите название вашей компании")]
		[MaxLength(50, ErrorMessage = "Максимальная длина 50 знаков")]
		public string CompanyName { get; set; }

		[UIHint("EditorMailReg")]
		[Display(Name = "Email")]
		[Required(ErrorMessage = "Введите email")]
		[EmailAddress(ErrorMessage = "Введите корректый email")]
		public string login { get; set; }

		[UIHint("EditorStringPosition")]
		[Display(Name = "Должность")]
		public string Appointment { get; set; }

		[UIHint("EditorPhone")]
		[Display(Name = "Номер телефона")]
		[StringLength(15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
		[Phone(ErrorMessage = "Некорректно введен номер")]
		public string PhoneNumber { get; set; }

	}
}
