using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Quartz.Job.Models
{
	public class SetDateInterval
	{
		[Display(Name = "Дата с")]
		[NotMinDate(ErrorMessage = "Не указана дата")]
		[UIHint("Date")]
		[PastDate(ErrorMessage = "Укажите дату в прошлом")]
		public DateTime DateFrom { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateTo { get; set; }

		[Display(Name = "по")]
		[NotMinDate(ErrorMessage = "Не указана дата")]
		[UIHint("Date")]
		[PastDate(ErrorMessage = "Укажите дату в прошлом")]
		public DateTime DateToUi
		{
			get { return DateTo == DateTime.MinValue ? DateTime.MinValue : DateTo.AddDays(-1); }
			set { DateTo = value.AddDays(1); }
		}

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string JobName { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string JobGroup { get; set; }
	}
}