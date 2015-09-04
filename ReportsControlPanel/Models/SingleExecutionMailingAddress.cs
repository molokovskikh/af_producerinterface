using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using NHibernate.Mapping;

namespace ReportsControlPanel.Models
{
	[Description("Почтовый адрес для единоразового запуска отчета"), Model("mailing_addresses", "reports")]
	public class SingleExecutionMailingAddress : BaseModel
	{
		[Map(PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[Map("mail"),Description("Адрес электронной почты")]
		public virtual string Email { get; set; } 

		/// <summary>
		/// Отчет за которым закреплен адрес
		/// </summary>
		[Description("Отчет"), BelongsTo("GeneralReport")]
		public virtual GeneralReport GeneralReport { get; set; }
    }
}