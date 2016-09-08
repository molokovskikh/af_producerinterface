using System;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Models
{
	public class Producer
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
	public class Job
	{
		public virtual int Id { get; set; }
		[Display(Name = "Формировать отчет")]
		public virtual string SchedName { get; set; }
		public virtual string JobName { get; set; }
		public virtual string JobGroup { get; set; }
		[Display(Name = "Название")]
		public virtual string CustomName { get; set; }
		[Display(Name = "Расписание")]
		public virtual string Scheduler { get; set; }
		[Display(Name = "Тип и параметры")]
		public virtual Reports ReportType { get; set; }
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate { get; set; }
		[Display(Name = "Изменен")]
		public virtual DateTime LastModified { get; set; }
		[Display(Name = "Статус")]
		public virtual DisplayStatus DisplayStatus { get; set; }
		[Display(Name = "Запуск")]
		public virtual DateTime? LastRun { get; set; }
		public virtual bool Enable { get; set; }

		[Display(Name = "Производитель")]
		public virtual Producer Producer { get; set; }
		[Display(Name = "Создатель")]
		public virtual User Owner { get; set; }
	}
}