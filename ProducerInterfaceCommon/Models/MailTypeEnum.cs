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

        [Display(Name = "Создание Промо-Акции")]
        CreatePromotion = 5,

        [Display(Name = "Изменение Промо-Акции")]
        EditPromotion = 6,

        [Display(Name = "Подтверждение Промо-Акции")]
        StatusPromotion = 7
    }
}
