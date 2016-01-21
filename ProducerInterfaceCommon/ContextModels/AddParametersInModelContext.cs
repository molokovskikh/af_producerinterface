using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ContextModels
{

    public enum DisplayStatus
    {

        [Display(Name = "Не запускался")]
        New = 0,

        [Display(Name = "Отчёт готовится")]
        Processed = 1,

        [Display(Name = "Отчёт готов")]
        Ready = 2,

        [Display(Name = "Нет данных для построения отчёта")]
        Empty = 3,

        [Display(Name = "В процессе подготовки произошла ошибка")]
        Error = 4
    }

    public enum Reports
    {
        [Display(Name = "Рейтинг товаров")]
        ProductRatingReport = 1,

        [Display(Name = "Рейтинг аптек")]
        PharmacyRatingReport = 2,

        [Display(Name = "Рейтинг поставщиков")]
        SupplierRatingReport = 3
    }

    [MetadataType(typeof(JobExtendMetaData))]
    public partial class jobextend
    {
        public Reports ReportTypeEnum
        {
            get { return (Reports)ReportType; }
            set { ReportType = (int)value; }
        }

        public DisplayStatus DisplayStatusEnum
        {
            get { return (DisplayStatus)DisplayStatus; }
            set { DisplayStatus = (int)value; }
        }

    }

    public class JobExtendMetaData
    {
        [ScaffoldColumn(false)]
        public string SchedName { get; set; }

        [ScaffoldColumn(false)]
        public string JobName { get; set; }

        [ScaffoldColumn(false)]
        public string JobGroup { get; set; }

        [Display(Name = "Название")]
        public string CustomName { get; set; }

        [Display(Name = "Расписание")]
        public string Scheduler { get; set; }

        [ScaffoldColumn(false)]
        public int ReportType { get; set; }

        [Display(Name = "Тип и Параметры")]
        public Reports ReportTypeEnum { get; }

        [ScaffoldColumn(false)]
        public long ProducerId { get; set; }

        [Display(Name = "Создатель")]
        public string Creator { get; set; }

        [ScaffoldColumn(false)]
        public System.DateTime CreationDate { get; set; }

        //[Display(Name = "Последние изменения")]
        [ScaffoldColumn(false)]
        public System.DateTime LastModified { get; set; }

        [ScaffoldColumn(false)]
        public int DisplayStatus { get; set; }

        [Display(Name = "Статус")]
        public DisplayStatus DisplayStatusEnum { get; }

        [Display(Name = "Запуск")]
        public System.DateTime LastRun { get; set; }

        [ScaffoldColumn(false)]
        public bool Enable { get; set; }
    }

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

    public class SecuredControllerAttribute : Attribute
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

    public partial class ProducerUser
    {
        [Display(Name = "Списко прав")]
        [UIHint("LongList")]
        public List<long> UserPermission { get; set; }

        public List<OptionElement> ListPermission { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListSelectedPermission { get; set; }
    }
    public partial class ControlPanelGroup
    {
        public List<int> ListPermission { get; set; }
        public List<long> ListUser { get; set; }
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
    public partial class promotions
    {
        public List<OptionElement> GlobalDrugList { get; set; }
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
    [MetadataType(typeof(promotionsMetaData))]
    public partial class promotions
    {

    }
    public class promotionsMetaData
    {
        public long Id { get; set; }
        public System.DateTime UpdateTime { get; set; }
  
        public Nullable<long> AdminId { get; set; }
        public long ProducerId { get; set; }
        public long ProducerUserId { get; set; }
        public string Annotation { get; set; }
        public string PromoFile { get; set; }
        public bool AgencyDisabled { get; set; }
        public string Name { get; set; }
        public decimal RegionMask { get; set; }


        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> Begin { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> End { get; set; }

        public bool Status { get; set; }

        public virtual ProducerUser ProducerUser { get; set; }

        public virtual ICollection<promotionToDrug> promotionToDrug { get; set; }

    } 
}
