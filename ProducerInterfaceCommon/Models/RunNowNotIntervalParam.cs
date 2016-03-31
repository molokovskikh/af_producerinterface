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
			// по умолчанию - за вчера
			var now = DateTime.Now;
			var yesterday = now.AddDays(-1);
      DateFrom = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day);
		}
	}
}