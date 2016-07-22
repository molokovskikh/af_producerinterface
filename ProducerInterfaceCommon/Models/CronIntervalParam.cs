using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	public enum IntervalType
	{
		[Display(Name = "За предыдущий месяц")] ByPreviousMonth,
		[Display(Name = "Интервал отчета (дни) от текущей даты")] Interval,
	}

	[Serializable]
	public class CronIntervalParam : CronParam, IInterval
	{
		[Display(Name = "Период подготовки отчета")]
		public IntervalType IntervalType { get; set; }

		[Display(Name = "Интервал отчета (дни) от текущей даты")]
		public int? Interval { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateFrom
		{
			get
			{
				// если за предыдущий месяц - с: 00:00:00 первый день предыдущего месяца
				if (IntervalType == IntervalType.ByPreviousMonth)
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
				if (IntervalType == IntervalType.ByPreviousMonth)
					return new DateTime(now.Year, now.Month, 1);
				// если за X предыдущих дней от момента запуска - по: 00:00:00 сегодня
				else
					return new DateTime(now.Year, now.Month, now.Day);
			}
			set { }
		}

		public CronIntervalParam()
		{
			IntervalType = IntervalType.ByPreviousMonth;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (IntervalType == IntervalType.Interval && !Interval.HasValue)
				errors.Add(new ErrorMessage("Interval", "Не указан интервал"));
			return errors;
		}
	}
}