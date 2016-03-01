using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.Interface.Registration
{
    /* Модель для регистрации первого пользователя от производителя */
    public class RegViewModel
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
        [MaxLength(30,ErrorMessage ="Максимальная длина 30 знаков")]      
        public string OtherName { get; set; }

        [UIHint("EditorMailReg")]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректый email")]
        public string login { get; set; }

        [Display(Name = "Должность")]
        [UIHint("IntApointment")]
        [Required(ErrorMessage = "Выберите должность")]
        public int AppointmentId { get; set; }

        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность")]
        public string Appointment { get; set; }

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]
        [Phone(ErrorMessage = "Некорректно введен номер")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
        public string PhoneNumber { get; set; }


        /* эти два поля будут скрытыми на странице, пользователь ранее выбрал компанию производителя */
        public long ProducerId { get; set; }
        public string ProducerName { get; set; }
    }
}
