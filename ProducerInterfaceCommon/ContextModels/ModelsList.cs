using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ContextModels
{

    public class Producer
    {
        public virtual string Name { get; set; }
        public virtual IList<ProducerUser> Users { get; set; }
        public virtual List<Drug> Drugs { get; set; }
    }

    public class Drug
    {
        // drug NAME table assortment 
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual MNN MNN { get; set; }
        public virtual DateTime UpdateTime { get; set; }
    }

    public class MNN
    {
        public virtual string Value { get; set; }
        public virtual string RussianValue { get; set; }
        public virtual DateTime UpdateTime { get; set; }
    }

    public class PromotionValidation
    {
        public long Id { get; set; }

        public PromotionValidation()
        {

        }

        [Display(Name = "Заголовок")]
        [Required(ErrorMessage = "Название акции не заполнено")]
        public string Name { get; set; }

        [Display(Name = "Содержание")]
        [Required(ErrorMessage = "Добавьте содержание")]
        public string Annotation { get; set; }

        [UIHint("Date")]
        [Required(ErrorMessage = "Укажите дату начала Акции")]
        [Display(Name = "Дата начала акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> Begin { get; set; }

        [UIHint("Date")]
        [Required(ErrorMessage = "Укажите дату окончания акции")]
        [Display(Name = "Дата окончания акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> End { get; set; }
        public bool Status { get; set; }

        [UIHint("LongList")]
        [Required(ErrorMessage = "Добавьте лекарства участвующие в акции")]
        public virtual List<long> DrugList { get; set; }     
    }

    public class ListGroupView
    {
        public int Id { get; set; }
        public int CountUser { get; set; }
        public string Description { get; set; }
        public string NameGroup { get; set; }

        public string[] Users { get; set; }
        public string[] Permissions { get; set; }
    }

    public class SearchPromotion
    {
        [Required(ErrorMessage = "Не выбран производитель")]
        public long IdProducer { get; set; }
    }

    public class ListUserView
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int CountGroup { get; set; }
        public string[] Groups { get; set; }
        public int CountPermissions { get; set; }
        public string[] ListPermission { get; set; }
    }

    public class LoginValidation
    {
        [UIHint("EditorMail")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "введите E-mail")]
        [EmailAddress(ErrorMessage = "Введите корректный E-mail")]
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
        [EmailAddress(ErrorMessage = "Введите корректый E-mail")]
        public string login { get; set; }


        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность: ")]
        [Required(ErrorMessage = "Введите должность")]
        public string Appointment { get; set; }


        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Выберите вашу компанию: ")]
        public long Producers { get; set; }
    }

    public class SearchProducerReportsModel
    {
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Название компании производителя: ")]
        public long Producers { get; set; }
    }

    public class PasswordUpdate
    {
        [UIHint("EditorMail")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Введите E-mail")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введите корректый E-mail")]
        public string login { get; set; }
    }

    public class OptionElement
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class SecuredControllerAttribute : Attribute
    {

    }

}
