using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Quartz.Job.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")]
	public abstract class IntervalReport : Report
	{
		[Display(Name = "За предыдущий месяц")]
		[UIHint("Bool")]
		public bool ByPreviousMonth { get; set; }

		[Display(Name = "Дней от текущей даты")]
		[UIHint("Interval")]
		public int? Interval { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateFrom { get; set; }

		[ScaffoldColumn(false)]
		public DateTime DateTo { get; set; }

		public IntervalReport()
		{
			ByPreviousMonth = true;
			Interval = 7;
		}

		public override Report Process(Report param, JobKey key, bool runNow)
		{
			var castparam = (IntervalReport)param;
			// если запуск вручную - не пересчитываем даты
			if (runNow)
				return castparam;

			// если запуск по крону - пересчитываем даты относительно момента запуска
			var now = DateTime.Now;
			// за предыдущий месяц
			if (castparam.ByPreviousMonth) {
				// по : 00:00:00 первый день текущего месяца
				castparam.DateTo = new DateTime(now.Year, now.Month, 1);
				// с: 00:00:00 первый день предыдущего месяца
				castparam.DateFrom = castparam.DateTo.AddMonths(-1);
			}
			// за X предыдущих дней от момента запуска
			else {
				// по: 00:00:00 сегодня
				castparam.DateTo = new DateTime(now.Year, now.Month, now.Day);
				// с: 00:00:00 за Interval дней
				castparam.DateFrom = castparam.DateTo.AddDays(-castparam.Interval.Value);
			}
			return castparam;
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