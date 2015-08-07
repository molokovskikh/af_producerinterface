using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ReportsControlPanel.Models
{
	[Model(Database = "reports",Table = "general_reports")]
	public class GeneralReport : BaseModel
	{
		[Map(Column ="GeneralReportCode", PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[BelongsTo("PayerID")]
		public virtual Payer Payer { get; set; }

		[Map]
		public virtual string ReportFileName { get; set; }

		[Map]
		public virtual string ReportArchName { get; set; }

		[Map("EMailSubject")]
		public virtual string Title { get; set; }
	}
}