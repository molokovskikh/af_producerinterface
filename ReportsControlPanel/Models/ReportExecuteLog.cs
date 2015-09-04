using System;
using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ReportsControlPanel.Models
{
	[Description("Статистика запуска отчета"), Model(Database = "Logs", Table = "ReportExecuteLogs")]
	public class ReportExecuteLog : BaseModel
	{
		[Map(PrimaryKey = true)]
		public override int Id { get; set; }

		[Description("Час запуска"), Map]
		public virtual DateTime? StartTime { get; set; }

		[Description("Минута запуска"), Map]
		public virtual DateTime? EndTime { get; set; }

		[Description("Отчет"), BelongsTo(Column = "GeneralReportCode"), ValidatorNotNull]
		public virtual GeneralReport GeneralReport { get; set; }

		[Description("Ошибка завершения"),Map("EndError")]
		public virtual bool IsFailed { get; set; }
	}
}