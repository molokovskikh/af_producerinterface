using System;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class CronNotIntervalParam : CronParam, INotInterval
	{

		[Display(Name = "Отчет готовится по данным")]
		[UIHint("ByPreviousDay")]
		public bool ByPreviousDay { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateFrom
		{
			get
			{
				var now = DateTime.Now;
				// если за предыдущий день
				if (ByPreviousDay) 
					return DateFrom = new DateTime(now.Year, now.Month, now.Day).AddDays(-1);
				// если на момент запуска
				else
					return DateFrom = new DateTime(now.Year, now.Month, now.Day);
			}
			set { }
		}

		public CronNotIntervalParam()
		{
			ByPreviousDay = true;
		}

	}
}