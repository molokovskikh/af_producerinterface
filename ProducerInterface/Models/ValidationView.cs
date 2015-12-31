using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Quartz.Job.Models;

namespace ProducerInterface.Models
{
    public class ValidationView
    {
    }

    public class PromotionValidation
    {
        public long Id { get; set; }

        public PromotionValidation()
        {
               
        }

        [Display(Name = "Заголовок")]
        [Required(ErrorMessage = " Название акции не заполнено")]
        public string Name { get; set; }

        [Display(Name = "Содержание")]
        [Required(ErrorMessage = "Добавьте содержание")]
        public string Annotation { get; set; }

        [UIHint("Date")]
        [Display(Name = "Дата начала акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> Begin { get; set; }

        [UIHint("Date")]
        [Display(Name = "Дата окончания акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> End { get; set; }
        public bool Status { get; set; }

        [UIHint("LongList")]
        [Required(ErrorMessage = "Добавьте лекарства участвующие в акции")]
        public virtual List<long> DrugList { get; set; }

        //public override List<ErrorMessage> Validate()
        //{
        //    var errors = new List<ErrorMessage>();           
        //    if (arrInput.Length != 5)
        //        errors.Add(new ErrorMessage("", "Неправильный формат строки Cron"));
        //    return errors;
        //}

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