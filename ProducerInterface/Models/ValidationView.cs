using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProducerInterface.Models
{
    public class ValidationView
    {
    }

    public class LoginValidation
    {
        [UIHint("EditorMail")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "введите E-mail")]
        [EmailAddress(ErrorMessage = "Введите корректынй E-mail")]
        public string login { get; set; }

        [UIHint("Editor_Password")]
        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        public string password { get; set; }

    }

    public class RegistrerValidation
    {
      
        [UIHint("EditorString")]
        [Display(Name = "ФИО:")]
        [Required(ErrorMessage = "Введите ФИО")]
        public string Name { get; set; }

     
        [UIHint("EditorMailReg")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Введите E-mail")]
        [EmailAddress(ErrorMessage = "Введите корректынй E-mail")]
        public string login { get; set; }
                   

        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность: ")]
        [Required(ErrorMessage = "Введите должность")]
        public string Appointment { get; set; }

      
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Выберите вышу компанию: ")]
        public long Producers { get; set; }
    }

    public class PasswordUpdate
    {
        [UIHint("EditorMail")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Введите E-mail")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введите корректынй E-mail")]
        public string login { get; set; }
    }

    public class OptionElement
    {
        public string Value { get; set; }

        public string Text { get; set; }
    }
    public class OptionElement2
    {
        public long Value { get; set; }

        public string Text { get; set; }
    }
}