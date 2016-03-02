using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ProducerInterfaceCommon.ViewModel.ControlPanel.GlobalAccount
{
    public class RegistrationCustomProfile
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Укажите логин (Email)")]
        [EmailAddress(ErrorMessage ="Некореектно введен Email (логин)")]
        [Display(Name = "Логин")]
        [UIHint("String6")]
        public string Login { get; set; }

        [UIHint("SelectPost")]
        [Required(ErrorMessage = "Выберите должность")]
        [Display(Name = "Должность")]
        public int AppointmentId { get; set; }

        [UIHint("string12")]
        [Required(ErrorMessage = "Укажите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [UIHint("string6")]
        [Required(ErrorMessage = "Укажите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [UIHint("string6")]
        [Display(Name = "Отчество")]
        public string OtherName { get; set; }

        [UIHint("string6")]
        [Display(Name = "Номер телефона")]
        public string Phone { get; set; }

        [UIHint("string12")]
        [Display(Name = "Название компании")]
        public string CompanyName { get; set; }                                    
    
        public long CompanyId { get; set; }

        [UIHint("GroupSelectListInt")]
        [Display(Name="Выберите группы доступа")]
        [Required(ErrorMessage ="Выберите группы")]
        public List<int> SelectedGroupList { get; set; }
    }
}
