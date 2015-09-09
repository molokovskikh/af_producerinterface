using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using NHibernate.Mapping;

namespace ProducerInterface.Models
{
	[Model("catalog","catalogs")]
	public class Drug : BaseModel
	{
		[Map]
		public virtual string Name { get; set; }

		[HasMany(ManyToMany = true, Table = "assortment")]
		public virtual IList<Producer> Producers { get; set; }

		[BelongsTo(Column = "NameId")]
		public virtual DrugFamily DrugFamily { get; set; }
	}
}