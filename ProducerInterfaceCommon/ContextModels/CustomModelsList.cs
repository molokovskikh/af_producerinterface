using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ContextModels
{
    public class SortingPagingInfo
    {   
        public int PageCount { get; set; }
        public int CurrentPageIndex { get; set; }
    }

    public class Producer
    {
        public virtual string Name { get; set; }
        public virtual IList<Account> Users { get; set; }
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

    public class AdminAutentification
    {
        public long IdProducerUser { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
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
        [Required(ErrorMessage = "Укажите дату начала акции")]
        [Display(Name = "Дата начала акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
                           
        public Nullable<System.DateTime> Begin { get; set; }

        [UIHint("Date")]
        [Required(ErrorMessage = "Укажите дату окончания акции")]
        [Display(Name = "Дата окончания акции")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> End { get; set; }
        public bool Status { get; set; }

        [UIHint("FileUpLoad")]
        public HttpPostedFileBase File { get; set; }

        [UIHint("LongList")]
        [Required(ErrorMessage = "Список препаратов, участвующих в акции")]
        public virtual List<long> DrugList { get; set; }

        [UIHint("LongList")]
        [Required(ErrorMessage = "Выберите регион")]
        public List<long> RegionList { get; set; }
        
        public int? PromotionFileId { get; set; }
        public string PromotionFileName { get; set; }

        public byte[] ImageFile { get; set; }
    }

    public partial class promotions
    {
        public List<long> RegionList { get; set; }
    }

    public class PromotionFile
    {
        public long Id { get; set; }
        public int FileId { get; set; }
        public string ImageFile { get; set; }      
    }

    public class ListGroupView
    {
        public int Id { get; set; }
        public int CountUser { get; set; }
        public string Description { get; set; }
        public string NameGroup { get; set; }

        public string[] Users { get; set; }

        public string[] Permissions { get; set; }
        public List<UsersViewInChange> ListUsersInGroup {get;set;}

    }

    public class UsersViewInChange
    {
        public string Name { get; set; }
        public string eMail { get; set; }
        public string ProducerName { get; set; }
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
        [Display(Name = "Email")]
        [Required(ErrorMessage = "введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string login { get; set; }

        [UIHint("Editor_Password")]
        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Введите пароль")]
        public string password { get; set; }
    }

    public class RegistrationAccountValidation
    {
        [UIHint("EditorProducer")]
        [Required(ErrorMessage = "Выберите компанию")]
        [Display(Name = "Выберите вашу компанию: ")]
        public long Producers { get; set; }
    }

    public class ProfileValidation
    {
        [UIHint("EditorString")]
        [Display(Name = "ФИО")]
        [Required(ErrorMessage = "Введите ФИО")]
        public string Name { get; set; }

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

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]       
        [Phone(ErrorMessage = "Некорректно введен номер")]
          [StringLength(15,MinimumLength = 15, ErrorMessage ="Корректно заполните номер телефона")]
        public string PhoneNumber { get; set; }
    }

    public class RegistrerValidation
    {

        [UIHint("EditorString")]
        [Display(Name = "ФИО")]
        [Required(ErrorMessage = "Введите ФИО")]
        public string Name { get; set; }


        [UIHint("EditorMailReg")]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректый email")]
        public string login { get; set; }


        [Display(Name = "Должность")]
        [UIHint("IntApointment")]
        [Required(ErrorMessage = "Должность")]
        public int AppointmentId { get; set; }

        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность")]
        public string Appointment { get; set; }

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]      
        [Phone(ErrorMessage ="Некорректно введен номер")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "Корректно заполните номер телефона")]
        public string PhoneNumber { get; set; }

        public long Producers { get; set; }
        public string ProducerName { get; set; }

    }

    public class RegisterDomainValidation
    {
        public long Producers { get; set; }
        public string ProducerName { get; set; }

        [UIHint("EditorString")]
        [Display(Name = "ФИО")]
        [Required(ErrorMessage = "Введите ФИО")]
        public string Name { get; set; }

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
    }

    public class RegisterCustomValidation
    {

        [UIHint("EditorString")]
        [Display(Name = "ФИО")]
        [Required(ErrorMessage = "Введите ФИО")]
        public string Name { get; set; }

        [UIHint("EditorString")]
        [Display(Name = "Название компании")]
        [Required(ErrorMessage = "Укажите название вашей компании")]
        public string CompanyName { get; set; }

        [UIHint("EditorMailReg")]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректый email")]
        public string login { get; set; }

        [UIHint("EditorStringPosition")]
        [Display(Name = "Должность")]
        [Required(ErrorMessage = "Введите должность")]
        public string Appointment { get; set; }

        [UIHint("EditorPhone")]
        [Display(Name = "Номер телефона")]
        [StringLength(15,MinimumLength = 15, ErrorMessage ="Корректно заполните номер телефона")]
        [Phone(ErrorMessage = "Некорректно введен номер")]
        public string PhoneNumber { get; set; }

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
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введите email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введите корректый email")]
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
