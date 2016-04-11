using ProducerInterfaceCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ContextModels
{
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
		public int PageCount { get; set; }
		public int CurrentPageIndex { get; set; }
	}

	public partial class PromotionsInRegionMask_Result
	{
		public long Id { get; set; }
		public decimal RegionMask { get; set; }
	}

	public class MailFormUi
	{
		public int Id { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Description { get; set; }
		public List<int> MediaFiles { get; set; }
		public List<int> AllMediaFiles { get; set; }

		public MailFormUi()
		{
			MediaFiles = new List<int>();
			AllMediaFiles = new List<int>();
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

	public class PasswordUpdate
	{
		[UIHint("EditorMail")]
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
