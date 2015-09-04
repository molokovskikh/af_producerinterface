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
	public enum ContactType
	{
		[Description("E-mail")]
		Email = 0,
		[Description("Телефон")]
		Phone = 1,
		[Description("Почтовый адрес")]
		MailingAddress = 2,
		[Description("Факс")]
		Fax = 3,
	}

	[Model("contacts","contacts")]
	public class Contact : BaseModel
	{
		[Map(PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[Map]
		public virtual ContactType Type { get; set; }

		[Map]
		public virtual string ContactText { get; set; }

		[Map]
		public virtual string Comment { get; set; }
	}
}