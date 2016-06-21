using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	public enum MailType
	{
		[Display(Name = "Регистрация в системе")]
		Registration = 1,

		[Display(Name = "Смена пароля")]
		PasswordChange = 2,

		[Display(Name = "Восстановление пароля")]
		PasswordRecovery = 3,

		[Display(Name = "Ошибка при формировании отчета")]
		ReportError = 4,

		[Display(Name = "Создание акции")]
		CreatePromotion = 5,

		[Display(Name = "Изменение акции")]
		EditPromotion = 6,

		[Display(Name = "Подтверждение акции")]
		StatusPromotion = 7,

		[Display(Name = "Шапка/Подпись")]
		HeaderFooter = 8,

		[Display(Name = "Нет данных для формировании отчета")]
		EmptyReport = 9,

		[Display(Name = "Автоматическая рассылка отчетов")]
		AutoPostReport = 10,

		[Display(Name = "Ручная рассылка отчетов")]
		ManualPostReport = 11,

		[Display(Name = "Подтверждение регистрации пользователя без производителя")]
		AccountVerification = 12,

		[Display(Name = "Изменение свойств препарата")]
		CatalogPKU = 13,

		[Display(Name = "Изменение описания препарата")]
		CatalogDescription = 14,

		[Display(Name = "Именение МНН препарата")]
		CatalogMNN = 15,

		[Display(Name = "Реакция на давно не запускавшийся отчет")]
		CallForDelete = 16,

		[Display(Name = "Запрос регистрации")]
		ProducerRequest = 17,

		[Display(Name = "Отклонение правки в каталог")]
		RejectCatalogChange = 18,

		[Display(Name = "Действие с новостью")]
		NewsAction = 19
	}
}
