using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class CronIntervalParam : CronParam, IInterval
	{

		[Display(Name = "За предыдущий календарный месяц")]
		[UIHint("Bool")]
		public bool ByPreviousMonth { get; set; }

		[Display(Name = "Дней от текущей даты")]
		[UIHint("Interval")]
		public int? Interval { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateFrom
		{
			get
			{
				var now = DateTime.Now;
				// если за предыдущий месяц - с: 00:00:00 первый день предыдущего месяца
				if (ByPreviousMonth) 
					return DateTo.AddMonths(-1);
				// если за X предыдущих дней от момента запуска - с: 00:00:00 за Interval дней
				else 
					return DateTo.AddDays(-Interval.GetValueOrDefault());
			}
			set { }
		}

		[ScaffoldColumn(false)]
		public DateTime DateTo
		{
			get
			{
				var now = DateTime.Now;
				// если за предыдущий месяц - по 00:00:00 первый день текущего месяца
				if (ByPreviousMonth) 
					return new DateTime(now.Year, now.Month, 1);
				// если за X предыдущих дней от момента запуска - по: 00:00:00 сегодня
				else 
					return new DateTime(now.Year, now.Month, now.Day);
			}
			set { }
		}

		public CronIntervalParam()
		{
			ByPreviousMonth = true;
			//Interval = 7;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (!ByPreviousMonth && !Interval.HasValue)
				errors.Add(new ErrorMessage("Interval", "Не указан интервал"));
			return errors;
		}


	}
}