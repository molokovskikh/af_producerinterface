using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ReportsControlPanel.Models
{
	[Description("Генеральный отчет")]
	[Model(Database = "reports",Table = "general_reports")]
	public class GeneralReport : BaseModel
	{
		[Map(Column ="GeneralReportCode", PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[Map("EMailSubject"), ValidatorNotEmpty]
		public virtual string Title { get; set; }

		[BelongsTo("PayerID")]
		public virtual Payer Payer { get; set; }

		[Map("Allow")]
		public virtual bool Enabled { get; set; }

	}
}