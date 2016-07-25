using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Profile
{
    public class ProfileValidation
    {
        [UIHint("EditorString")]
        [Display(Name = "Фамилия")]
        [Required(ErrorMessage = "Введите Фамилию")]
        public string LastName { get; set; }

        [UIHint("EditorString")]
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Введите Имя")]
        public string FirstName { get; set; }

        [UIHint("EditorString")]
        [Display(Name = "Отчество")]
        public string OtherName { get; set; }

        [UIHint("EditorString")]
        [Display(Name = "Название компании")]
        public string CompanyName { get; set; }

        [Display(Name = "Email")]
        [UIHint("EditorStringMail")]
        [Required(ErrorMessage = "Заполните email")]
        public string Mailname { get; set; }

        [Display(Name = "Доменное имя")]
        [UIHint("IntMailDomain")]
        [Required(ErrorMessage = "Укажите домен")]
        public int? EmailDomain { get; set; }

        [Display(Name = "Должность")]
        [UIHint("IntApointment")]
        [Required(ErrorMessage = "Должность")]
        public int? AppointmentId { get; set; }

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]
        [Phone(ErrorMessage = "Некорректно введен номер")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
