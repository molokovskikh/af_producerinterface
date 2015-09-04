
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
	[Description("Недельное расписание отчета")]
	public abstract class Schedule : BaseModel
	{
		[Map("Id", PrimaryKey = true)]
		public override int Id { get; set; }

		[Description("Час запуска"), Map, ValidatorInRange(0, 24)]
		public virtual int Hour { get; set; }

		[Description("Минута запуска"), Map, ValidatorInRange(0, 60)]
		public virtual int Minute { get; set; }

		[Description("Отчет"), BelongsTo, ValidatorNotNull]
		public virtual GeneralReport GeneralReport { get; set; }

		/// <summary>
		/// Флаг синхронизации. Если false, то триггер планировщика Windows не будет сихронизирован с расписанием,
		/// при изменении расписания. Флаг должен быть включен только в крайнем случае.
		/// </summary>
		public virtual bool NoSync { get; set; }

		/// <summary>
		/// Если удаляется расписание, то и удаляется связанный с ней триггер планировщика Windows
		/// </summary>
		/// <returns></returns>
		public override bool BeforeDelete()
		{
			DeleteTrigger();
			return true;
		}

		/// <summary>
		/// Если изменяется расписание, то и изменяется связанный с ней триггер планировщика Windows
		/// </summary>
		/// <returns></returns>
		public override bool BeforeSave()
		{
			if (!NoSync)
				UpdateTrigger();
			return true;
		}

		/// <summary>
		/// Обновление триггра планировщика Windows, связанного с расписанием
		/// </summary>
		protected void UpdateTrigger()
		{
			var task = GeneralReport.GetTask();
			var trigger = GetTrigger(task);
			CopyPropertiesToTrigger(trigger);

			ScheduleHelper.UpdateTaskDefinition(task.TaskService, task.Folder, GeneralReport.Id, task.Definition, "GR");
		}

		/// <summary>
		/// Копирование настроек расписания в триггер планировщика Windows
		/// </summary>
		/// <param name="trigger">Триггер планировщика Windows</param>
		public abstract void CopyPropertiesToTrigger(Trigger trigger);

		/// <summary>
		/// Копирование настроек расписания из триггера планировщика Windows
		/// </summary>
		/// <param name="obj">Триггер планировщика Windows</param>
		public abstract void CopyPropertiesFromTrigger(Trigger obj);

		/// <summary>
		/// Получение типа триггера, который лежит в основе графика
		/// </summary>
		/// <returns></returns>
		public abstract TaskTriggerType GetTriggerType();

		/// <summary>
		/// Получешение триггера планировщика Windows для данного отчета
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private Trigger GetTrigger(Task task)
		{
			Trigger trigger;
			if (IsNew())
			{
				trigger = task.Definition.Triggers.AddNew(GetTriggerType());
			}
			else
				trigger = task.Definition.Triggers.FirstOrDefault(i => i.Id == Id.ToString());
			return trigger;
		}

		/// <summary>
		/// Удаление триггера планировщика Windows для данного отчета
		/// </summary>
		private void DeleteTrigger()
		{
			var task = GeneralReport.GetTask();
			var trigger = GetTrigger(task);
			if (trigger == null)
				return;
			var index = task.Definition.Triggers.IndexOf(trigger.Id);
			task.Definition.Triggers.RemoveAt(index);
			ScheduleHelper.UpdateTaskDefinition(task.TaskService, task.Folder, GeneralReport.Id, task.Definition, "GR");
		}


	}
}