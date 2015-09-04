using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using Microsoft.Win32.TaskScheduler;

namespace ReportsControlPanel.Models
{
	[Model,Description("Месячное расписание отчета")]
	public class MonthlySchedule : Schedule
	{
		[Description("Месяца запуска отчета"), HasMany, ValidatorNotEmpty]
		public virtual IList<MonthsOfTheYear> Months { get; set; } = new List<MonthsOfTheYear>();

		[Description("Дни запуска отчета"), HasMany, ValidatorNotEmpty]
		public virtual IList<int> Days { get; set; } = new List<int>();

		/// <summary>
		/// Копирование настроек расписания в триггер планировщика Windows
		/// </summary>
		/// <param name="trigger">Триггер планировщика Windows</param>
		public override void CopyPropertiesToTrigger(Trigger obj)
		{
			var trigger = (MonthlyTrigger)obj;
			trigger.StartBoundary = new DateTime(trigger.StartBoundary.Year, trigger.StartBoundary.Month, trigger.StartBoundary.Day, Hour, Minute, 0);

			//Переносим месяца
			MonthsOfTheYear newmonths = 0;
			foreach (var month in Months)
			{
				var monthoftheyaer = (MonthsOfTheYear)MonthsOfTheYear.April.GetType().Parse(month.ToString());
				newmonths |= monthoftheyaer;
			}
			trigger.MonthsOfYear = newmonths;

			//Переносим дни
			 trigger.DaysOfMonth = Days.ToArray();
		}

		/// <summary>
		/// Копирование настроек расписания из триггера планировщика Windows
		/// </summary>
		/// <param name="obj">Триггер планировщика Windows</param>
		public override void CopyPropertiesFromTrigger(Trigger obj)
		{
			var trigger = obj as MonthlyTrigger;
			var months = Enum.GetNames(typeof (MonthsOfTheYear)).ToList();
			Months.Clear();
			foreach (var name in months)
			{
				var month = (MonthsOfTheYear) Enum.Parse(typeof (MonthsOfTheYear), name);
				if ((trigger.MonthsOfYear & month) != month)
					continue;
				Months.Add(month);
			}

			Days.Clear();
			foreach (var day in trigger.DaysOfMonth)
			{
				Days.Add(day);
			}

			Hour = trigger.StartBoundary.Hour;
			Minute = trigger.StartBoundary.Minute;
		}

		/// <summary>
		/// Получение типа триггера, который лежит в основе графика
		/// </summary>
		/// <returns></returns>
		public override TaskTriggerType GetTriggerType()
		{
			return TaskTriggerType.Monthly;
		}
	}
}