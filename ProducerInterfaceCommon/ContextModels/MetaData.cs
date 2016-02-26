using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;

namespace ProducerInterfaceCommon.ContextModels
{

    public partial class AccountFeedBack
    {
        public string Url_
        {
            get
            {
                return this.Description.Split(new Char[] { '>' })[0];
            }
        }
        public string FeedBack_Text
        {
            get
            {
                return this.Description.Split(new Char[] { '>' })[2];
            }
        }
        public string Contact
        {
            get
            {
                return this.Description.Split(new Char[] { '>' })[1];
            }
        }       
    }

	public partial class NewsChange
	{
		public string GetDisplayNewsTypeChange
		{
			get { return Heap.AttributeHelper.DisplayName(ChangeNewsType); }
		}
		private NewsChanges ChangeNewsType
		{
			get { return (NewsChanges)TypeCnhange; }
		}
	}

	[MetadataType(typeof(NotificationToProducersMetaData))]
	public partial class NotificationToProducers
	{
   
	}

	public class NotificationToProducersMetaData
	{
		[Display(Name = "Оглавление")]
		[MaxLength(150)]
		[Required(ErrorMessage = "Заполните поле Оглавление")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Заполните поле Новость")]
		[MaxLength(10000)]
		[Display(Name = "Новость")]
		public string Description { get; set; }

		[Display(Name = "Дата публикации")]
		public Nullable<System.DateTime> DatePublication { get; set; }
	}

	[MetadataType(typeof(JobExtendMetaData))]
	[DisplayName("Отчет")]
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

		[Display(Name = "Формировать отчет")]
		public string Scheduler { get; set; }

		[ScaffoldColumn(false)]
		public int ReportType { get; set; }

		[Display(Name = "Тип и параметры")]
		public Reports ReportTypeEnum { get; }

		[ScaffoldColumn(false)]
		public long ProducerId { get; set; }

		[ScaffoldColumn(false)]
		public string CreatorId { get; set; }

		[ScaffoldColumn(false)]
		public System.DateTime CreationDate { get; set; }

		[Display(Name = "Изменен")]
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


    [MetadataType(typeof(AccountMetaData))]
    public partial class Account
	{
		[Display(Name = "IP-адрес")]
		public string IP { get; set; }

		[Display(Name = "Список прав")]
		[UIHint("LongList")]
		public List<long> UserPermission { get; set; }

		public List<OptionElement> ListPermission { get; set; }

		[UIHint("LongListPermission")]
		public List<long> ListSelectedPermission { get; set; }

		[UIHint("LongListPermissionTwo")]
		public List<long> ListPermissionTwo { get; set; }

		public TypeUsers UserType
		{
			get { return (TypeUsers)TypeUser; }
			set { TypeUser = (SByte)value; }
		}

        public long ID_LOG {
            get
            {
                if (_id_log > 0) return _id_log;
                else return Id;
            } set { _id_log = value; } }

        private long _id_log { get; set; }

	}

    public class AccountMetaData
    {

    }

	public partial class AccountGroup
	{
		[UIHint("LongListPermission")]
		public List<int> ListPermission { get; set; }
		[UIHint("LongListUser")]
		public List<long> ListUser { get; set; }

	}

	public partial class promotions
	{
		public List<OptionElement> GlobalDrugList { get; set; }
        public List<OptionElement> RegionnamesList
        {
            get; set;
        }

        public void GetRegionnamesList()
        {
            var cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
            var h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx, cntx.Account.Where(xxx => xxx.Id == this.Account.Id).First().Id);
            RegionnamesList = h.GetPromotionRegionNames(Convert.ToUInt64(this.RegionMask));
        }

	}

	[MetadataType(typeof(promotionsMetaData))]
	[DisplayName("Акция")]
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

		public virtual ICollection<promotionToDrug> promotionToDrug { get; set; }

	}

	[MetadataType(typeof(reportrunlogwithuserMetaData))]
	public partial class reportrunlogwithuser
	{
	}

	public partial class reportrunlogwithuserMetaData
	{
		public int Id { get; set; }
		public string JobName { get; set; }

		[Display(Name = "IP-адрес пользователя")]
		public string Ip { get; set; }

		[Display(Name = "Дата запуска")]
		[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy hh:mm:ss}")]
		public DateTime RunStartTime { get; set; }

		public bool RunNow { get; set; }

		[Display(Name = "Пользователь")]
		public string UserName { get; set; }
	}

	[MetadataType(typeof(JobExtendWithProducerMetaData))]
	public partial class jobextendwithproducer
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

	public class JobExtendWithProducerMetaData
	{
		[ScaffoldColumn(false)]
		public string SchedName { get; set; }

		[ScaffoldColumn(false)]
		public string JobName { get; set; }

		[ScaffoldColumn(false)]
		public string JobGroup { get; set; }

		[Display(Name = "Название")]
		public string CustomName { get; set; }

		[Display(Name = "Формировать отчет")]
		public string Scheduler { get; set; }

		[ScaffoldColumn(false)]
		public int ReportType { get; set; }

		[Display(Name = "Тип и параметры")]
		public Reports ReportTypeEnum { get; }

		[ScaffoldColumn(false)]
		public long ProducerId { get; set; }

		[ScaffoldColumn(false)]
		public string CreatorId { get; set; }

		[Display(Name = "Создатель")]
		public string Creator { get; set; }

		[ScaffoldColumn(false)]
		public System.DateTime CreationDate { get; set; }

		[Display(Name = "Изменен")]
		public System.DateTime LastModified { get; set; }

		[ScaffoldColumn(false)]
		public int DisplayStatus { get; set; }

		[Display(Name = "Статус")]
		public DisplayStatus DisplayStatusEnum { get; }

		[Display(Name = "Запуск")]
		public System.DateTime LastRun { get; set; }

		[ScaffoldColumn(false)]
		public bool Enable { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }
	}

}
