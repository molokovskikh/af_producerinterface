using System;
using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ReportsControlPanel.Models
{
	[Description("Статистика отсылки отчета"), Model(Database = "Logs", Table = "reportslogs")]
	public class ReportSendLog : BaseModel
	{
		[Map("RowId", PrimaryKey = true)]
		public override int Id { get; set; }

		[Description("Время отправки"), Map("LogTime")]
		public virtual DateTime? Time { get; set; }

		[Description("Адрес эл. почты"), Map]
		public virtual string Email { get; set; }

		[Description("Отчет"), BelongsTo(Column = "GeneralReportCode"), ValidatorNotNull]
		public virtual GeneralReport GeneralReport { get; set; }

		[Map]
		public virtual int SMTPID { get; set; }
	}
}