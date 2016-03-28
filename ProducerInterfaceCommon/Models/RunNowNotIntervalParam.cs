using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class RunNowNotIntervalParam : RunNowParam, INotInterval
	{
		[Display(Name = "на дату")]
		[NotMinDate(ErrorMessage = "Не указана дата")]
		[UIHint("Date")]
		[PastDate(ErrorMessage = "Укажите дату в прошлом")]
		public DateTime DateFrom { get; set; }

		public RunNowNotIntervalParam()
		{
			// по умолчанию - за сегодня
			var now = DateTime.Now;
			DateFrom = new DateTime(now.Year, now.Month, now.Day);
		}
	}
}