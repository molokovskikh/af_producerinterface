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
		[Display(Name = "����������� �����")]
		public virtual string SchedName { get; set; }
		public virtual string JobName { get; set; }
		public virtual string JobGroup { get; set; }
		[Display(Name = "��������")]
		public virtual string CustomName { get; set; }
		[Display(Name = "����������")]
		public virtual string Scheduler { get; set; }
		[Display(Name = "��� � ���������")]
		public virtual Reports ReportType { get; set; }
		[Display(Name = "���� ��������")]
		public virtual DateTime CreationDate { get; set; }
		[Display(Name = "�������")]
		public virtual DateTime LastModified { get; set; }
		[Display(Name = "������")]
		public virtual DisplayStatus DisplayStatus { get; set; }
		[Display(Name = "������")]
		public virtual DateTime? LastRun { get; set; }
		public virtual bool Enable { get; set; }

		[Display(Name = "�������������")]
		public virtual Producer Producer { get; set; }
		[Display(Name = "���������")]
		public virtual User Owner { get; set; }
	}
}