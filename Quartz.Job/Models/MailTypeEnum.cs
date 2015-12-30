using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
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
			ReportError = 4
		}
}
