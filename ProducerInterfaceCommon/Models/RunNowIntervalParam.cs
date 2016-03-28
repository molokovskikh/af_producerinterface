using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class RunNowIntervalParam : RunNowParam, IInterval
	{
		[Display(Name = "с")]
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

		public RunNowIntervalParam()
		{
			// по умолчанию - за предыдущий календарный месяц
			var now = DateTime.Now;
			DateTo = new DateTime(now.Year, now.Month, 1);
			DateFrom = DateTo.AddMonths(-1);
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (DateToUi < DateFrom)
				errors.Add(new ErrorMessage("DateToUi", "Дата \"с...\" должна быть меньше или равна дате \"по...\""));

			return errors;
		}

	}
}