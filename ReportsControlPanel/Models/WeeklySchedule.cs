using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using Common.Schedule;
using Microsoft.Win32.TaskScheduler;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Param;
using ReportsControlPanel.Components;

namespace ReportsControlPanel.Models
{
	[Description("Недельное расписание отчета"), Model(Database = "Reports", Table = "WeeklySchedule")]
	public class WeeklySchedule : Schedule
	{

		[Description("Дни запуска отчета"), HasMany, ValidatorNotEmpty]
		public virtual IList<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();


		/// <summary>
		/// Копирование настроек расписания в триггер планировщика Windows
		/// </summary>
		/// <param name="trigger">Триггер планировщика Windows</param>
		public override void CopyPropertiesToTrigger(Trigger obj)
		{
			var trigger = (WeeklyTrigger)obj;
			trigger.StartBoundary = new DateTime(trigger.StartBoundary.Year, trigger.StartBoundary.Month, trigger.StartBoundary.Day, Hour, Minute, 0);
			DaysOfTheWeek newdays = 0;
			foreach (var day in Days)
			{
				var dayoftheweek = (DaysOfTheWeek)DaysOfTheWeek.Monday.GetType().Parse(day.ToString());
				newdays |= dayoftheweek;
			}
			trigger.DaysOfWeek = newdays;
		}

		/// <summary>
		/// Копирование настроек расписания из триггера планировщика Windows
		/// </summary>
		/// <param name="obj">Триггер планировщика Windows</param>
		public override void CopyPropertiesFromTrigger(Trigger obj)
		{
			var trigger = obj as WeeklyTrigger;
			var schedule = this;
			schedule.Hour = trigger.StartBoundary.Hour;
			schedule.Minute = trigger.StartBoundary.Minute;

			var daysofweek = Enum.GetNames(typeof(DaysOfTheWeek)).ToList();
			daysofweek.Remove("AllDays");
			var sunday = daysofweek.First();
			daysofweek.RemoveAt(0);
			daysofweek.Add(sunday);
			schedule.Days.Clear();
			foreach (var name in daysofweek)
			{
				var dayoftheweek = (DaysOfTheWeek)DaysOfTheWeek.Monday.GetType().Parse(name);
				if ((trigger.DaysOfWeek & dayoftheweek) != dayoftheweek)
					continue;
				var dayofweek = (DayOfWeek)DayOfWeek.Monday.GetType().Parse(name);
				schedule.Days.Add(dayofweek);
			}
		}

		/// <summary>
		/// Получение типа триггера, который лежит в основе графика
		/// </summary>
		/// <returns></returns>
		public override TaskTriggerType GetTriggerType()
		{
			return TaskTriggerType.Weekly;
		}
	}
}