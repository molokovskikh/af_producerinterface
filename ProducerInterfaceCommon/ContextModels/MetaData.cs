using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ContextModels
{

	[MetadataType(typeof(drugformproducerMetaData))]
	public partial class drugformproducer
	{
	}

	public class drugformproducerMetaData
	{

		[ScaffoldColumn(false)]
		public long CatalogId { get; set; }

		[ScaffoldColumn(false)]
		public long ProducerId { get; set; }

		[ScaffoldColumn(false)]
		public long DrugFamilyId { get; set; }

		[Display(Name = "Наименование", Order = 10)]
		public string CatalogName { get; set; }

		[Display(Name = "Жизненно важные", Order = 20)]
		public bool VitallyImportant { get; set; }

		[Display(Name = "Обязательный ассортимент", Order = 30)]
		public bool MandatoryList { get; set; }

		[Display(Name = "ПКУ: наркотические и психотропные", Order = 40)]
		public bool Narcotic { get; set; }

		[Display(Name = "ПКУ: сильнодействующие и ядовитые", Order = 50)]
		public bool Toxic { get; set; }

		[Display(Name = "ПКУ: комбинированные", Order = 60)]
		public bool Combined { get; set; }

		[Display(Name = "Монобренд", Order = 70)]
		public bool Monobrend { get; set; }

	}

	[MetadataType(typeof(drugdescriptionMetaData))]
	public partial class drugdescription
	{
	}

	public class drugdescriptionMetaData
	{
		[HiddenInput(DisplayValue = false)]
		public long DescriptionId { get; set; }

		[Display(Name = "Наименование", Order = 10)]
		[UIHint("TextBox")]
		[Required]
		public string DescriptionName { get; set; }

		[Display(Name = "Английское наименование", Order = 20)]
		[UIHint("TextBox")]
		public string EnglishName { get; set; }

		[Display(Name = "Формакологическое действие", Order = 30)]
		[UIHint("TextArea")]
		public string PharmacologicalAction { get; set; }

		[Display(Name = "Состав", Order = 40)]
		[UIHint("TextArea")]
		public string Composition { get; set; }

		[Display(Name = "Показания к применению", Order = 50)]
		[UIHint("TextArea")]
		public string IndicationsForUse { get; set; }

		[Display(Name = "Способ применения и дозы", Order = 60)]
		[UIHint("TextArea")]
		public string Dosing { get; set; }

		[Display(Name = "Взаимодействие", Order = 70)]
		[UIHint("TextArea")]
		public string Interaction { get; set; }

		[Display(Name = "Побочные эффекты", Order = 80)]
		[UIHint("TextArea")]
		public string SideEffect { get; set; }

		[Display(Name = "Предостережения и противопоказания", Order = 90)]
		[UIHint("TextArea")]
		public string Warnings { get; set; }

		[Display(Name = "Форма выпуска", Order = 100)]
		[UIHint("TextArea")]
		public string ProductForm { get; set; }

		[Display(Name = "Условия хранения", Order = 110)]
		[UIHint("TextArea")]
		public string Storage { get; set; }

		[Display(Name = "Срок годности", Order = 120)]
		[UIHint("TextArea")]
		public string Expiration { get; set; }

		[Display(Name = "Дополнительно", Order = 130)]
		[UIHint("TextArea")]
		public string Description { get; set; }

	}

	[MetadataType(typeof(drugmnnMetaData))]
	public partial class drugmnn
	{
	}

	public class drugmnnMetaData
	{
		[HiddenInput(DisplayValue = false)]
		public long MnnId { get; set; }

		[Display(Name = "Международное непатентованное наименование")]
		[UIHint("TextBox")]
		[Required]
		public string Mnn { get; set; }

		[Display(Name = "Международное непатентованное наименование (рус.)")]
		[UIHint("TextBox")]
		public string RussianMnn { get; set; }
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

        [Display(Name = "Тип и параметры")]
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

   


 

    public partial class ProducerUser
    {
        [Display(Name = "IP адресс")]
        public string IP { get; set; }

        [Display(Name = "Список прав")]
        [UIHint("LongList")]
        public List<long> UserPermission { get; set; }

        public List<OptionElement> ListPermission { get; set; }

        [UIHint("LongListPermission")]
        public List<long> ListSelectedPermission { get; set; }
    }
    public partial class ControlPanelGroup
    {
        [UIHint("LongListPermission")]
        public List<int> ListPermission { get; set; }
        [UIHint("LongListUser")]
        public List<long> ListUser { get; set; }
    }
       
    public partial class promotions
    {
        public List<OptionElement> GlobalDrugList { get; set; }
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
