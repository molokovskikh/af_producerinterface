using ProducerInterfaceCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.ContextModels
{
	public class Statistics
	{
		[Display(Name = "производителей")]
		public int ProducerCount { get; set; }

		[Display(Name = "наблюдателей")]
		public int NotProducerCount { get; set; }

		[Display(Name = "акций")]
		public int ActionCount { get; set; }

		[Display(Name = "активных акций")]
		public int ActiveActionCount { get; set; }

		[Display(Name = "подтвержденных акций")]
		public int AcceptedActionCount { get; set; }

		[Display(Name = "пользователей")]
		public int UserCount { get; set; }

		[Display(Name = "активных")]
		public int ActiveUserCount { get; set; }

		[Display(Name = "новых")]
		public int NewUserCount { get; set; }

		[Display(Name = "запросов на регистрацию")]
		public int RequestUserCount { get; set; }

		[Display(Name = "заблокированных")]
		public int BlockedUserCount { get; set; }

		[Display(Name = "отчетов")]
		public int ReportCount { get; set; }

		[Display(Name = "необработанных сообщений")]
		public int FeedBackNewCount { get; set; }

		[Display(Name = "запросов на правку каталога")]
		public int CatalogChangeRequest { get; set; }
	}

	public class UserEdit
	{
		[HiddenInput(DisplayValue = false)]
		public long Id { get; set; }

		[Display(Name = "ФИО")]
		public string Name { get; set; }

		[Display(Name = "Сообщение от пользователя")]
		public string Message { get; set; }

		[Display(Name = "Статус")]
		public sbyte Status { get; set; }

		public List<SelectListItem> AllStatus { get; set; }

		[Display(Name = "Должность")]
		public int? AppointmentId { get; set; }

		public List<SelectListItem> AllAppointment { get; set; }

		[Display(Name = "Группы")]
		[Required(ErrorMessage = "Укажите хотя бы одну группу")]
		public List<int> AccountGroupIds { get; set; }

		public List<SelectListItem> AllAccountGroup { get; set; }

		[Display(Name = "Доступные регионы")]
		[Required(ErrorMessage = "Укажите хотя бы один регион")]
		public List<decimal> AccountRegionIds { get; set; }

		public List<SelectListItem> AllAccountRegion { get; set; }
	}

	public class UserFilter
	{
		[Display(Name = "ФИО")]
		public string UserName { get; set; }

		[Display(Name = "Логин")]
		public string Login { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Телефон")]
		public string Phone { get; set; }

		[Display(Name = "Тип пользователя")]
		public TypeUsers TypeUserEnum { get { return TypeUsers.ProducerUser; } }

		[Display(Name = "Статус")]
		public sbyte? Status { get; set; }

		public List<SelectListItem> AllStatus { get; set; }

		[Display(Name = "Должность")]
		public int? AppointmentId { get; set; }

		public List<SelectListItem> AllAppointment { get; set; }

		[Display(Name = "Группа")]
		public int? AccountGroupId { get; set; }

		public List<SelectListItem> AllAccountGroup { get; set; }

		[HiddenInput(DisplayValue = false)]
		public int CurrentPageIndex { get; set; }
	}

	public class LogItem
	{
		public DateTime LogTime { get; set; }
		public string OperatorName { get; set; }
		public string OperatorLogin { get; set; }
		public string OperatorHost { get; set; }
		public Operation OperationEnum { get; set; }
		public string PropertyName { get; set; }

		public string OperationName
		{
			get { return OperationEnum.DisplayName(); }
		}
	}

	public class CataloglogUiPlus : cataloglogui
	{
		public string BeforeUi { get; set; }

		public string AfterUi { get; set; }

		public string LogTimeUi
		{
			get { return LogTime.ToString("dd.MM.yyyy HH:mm:ss"); }
		}

		public string DateEditUi
		{
			get
			{
				return DateEdit.HasValue ? DateEdit.Value.ToString("dd.MM.yyyy HH:mm:ss") : "";
			}
		}
	}

	public class CatalogLogFilter
	{
		public int CurrentPageIndex { get; set; }

		[Display(Name = "Статус")]
		public int? Apply { get; set; }

		public List<SelectListItem> ApplyList { get; set; }

		public long? ApplyId { get; set; }

		public long? RejectId { get; set; }

		public string RejectComment { get; set; }
	}

	public class ReportDescriptionUI
	{
		public int Id { get; set; }

		[Display(Name = "Отчет")]
		public string Name { get; set; }

		[Display(Name = "Описание")]
		[Required(ErrorMessage = "Добавьте описание отчета")]
		public string Description { get; set; }

		[Display(Name = "Доступные регионы")]
		[Required(ErrorMessage = "Добавьте регионы")]
		public List<decimal> RegionList { get; set; }
	}

	public class SortingPagingInfo
	{
		public int PageCount
		{
			get
			{
				return (int)Math.Ceiling((decimal)ItemsCount / ItemsPerPage);
			}
		}

		public int CurrentPageIndex { get; set; }
		public int ItemsPerPage { get; set; }
		public int ItemsCount { get; set; }
	}

	public class MailFormUi
	{
		public int Id { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Description { get; set; }
		public List<Tuple<int, string>> MediaFiles { get; set; }
		public List<Tuple<int, string>> AllMediaFiles { get; set; }

		public MailFormUi()
		{
			MediaFiles = new List<Tuple<int, string>>();
			AllMediaFiles = new List<Tuple<int, string>>();
		}
	}

	public class AdminAutentification
	{
		public long IdProducerUser { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
	}

	public class AdminAccountValidation
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public sbyte TypeUser { get; set; }

		public decimal? RegionMask { get; set; }

		[Display(Name = "Список регионов")]
		[UIHint("LongListRegion")]
		[Required(ErrorMessage = "Выберите регионы")]
		public List<long> RegionListId { get; set; }

		[Display(Name = "Список групп")]
		[UIHint("IntListGroup")]
		[Required(ErrorMessage = "Выберите группы")]
		public List<int> GroupListId { get; set; }

		public virtual List<ProducerInterfaceCommon.ContextModels.AccountGroup> ListGroup { get; set; }

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

		[UIHint("LongList")]
		[Required(ErrorMessage = "Выберите поставщиков")]
		public List<long> SuppierRegions { get; set; }

		public int? PromotionFileId { get; set; }
		public string PromotionFileName { get; set; }

		public byte[] ImageFile { get; set; }
	}

	public partial class promotions
	{
		public List<long> RegionList { get; set; }
		public List<OptionElement> DrugList { get; set; }
	}

	public class PromotionFile
	{
		public long Id { get; set; }
		public int FileId { get; set; }
		public string ImageFile { get; set; }
	}

	public class ListUserView
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string[] Groups { get; set; }
		public string[] ListPermission { get; set; }
	}

	public class LoginValidation
	{
		[Display(Name = "Email")]
		[Required(ErrorMessage = "Введите email")]
		[EmailAddress(ErrorMessage = "Введите корректный email")]
		public string login { get; set; }

		[Display(Name = "Пароль")]
		[Required(ErrorMessage = "Введите пароль")]
		public string password { get; set; }
	}

	public class PasswordUpdate
	{
		[Display(Name = "Email")]
		[Required(ErrorMessage = "Введите email")]
		[DataType(DataType.EmailAddress, ErrorMessage = "Введите корректый email")]
		public string login { get; set; }
	}

	public class SendReport
	{
		[Required]
		[HiddenInput(DisplayValue = false)]
		public string jobName { get; set; }

		[Display(Name = "Email")]
		[Required(ErrorMessage = "Не указан email")]
		public List<string> MailTo { get; set; }

		public List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();
			if (MailTo != null && MailTo.Count > 0)
			{
				var ea = new EmailAddressAttribute();
				var ok = true;
				foreach (var mail in MailTo)
					ok = ok && ea.IsValid(mail);
				if (!ok)
					errors.Add(new ErrorMessage("MailTo", "Неверный формат email"));
			}

			return errors;
		}
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
