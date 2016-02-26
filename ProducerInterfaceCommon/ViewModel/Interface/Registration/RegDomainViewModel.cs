using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Registration
{
    /* Модель для регистрации второго и последующих пользователя(ей) от производителя */
    public class RegDomainViewModel
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
        public int EmailDomain { get; set; }

        [Display(Name = "Должность")]
        [UIHint("IntApointment")]
        [Required(ErrorMessage = "Должность")]
        public int AppointmentId { get; set; }

        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность")]
        public string Appointment { get; set; }

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]
        [Phone(ErrorMessage = "Некорректно введен номер")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
        public string PhoneNumber { get; set; }

        /* эти два поля будут скрытыми на странице, пользователь ранее выбрал компанию производителя (их не требуется проверять) */
        public long Producers { get; set; }
        public string ProducerName { get; set; }
    }
}
