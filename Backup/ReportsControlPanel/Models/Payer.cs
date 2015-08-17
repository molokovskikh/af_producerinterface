using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ReportsControlPanel.Models
{
	[Model(Database = "Billing", Table ="Payers")]
	public class Payer : BaseModel
	{
		[Map(Column = "PayerID", PrimaryKey = true)]
		public virtual int Id { get; set;}

		[Map(Column = "ShortName")]
		public virtual string Name { get; set; }
	}
}