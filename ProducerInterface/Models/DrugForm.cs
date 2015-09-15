using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Model("catalogforms", "catalogs")]
	public class DrugForm : BaseModel
	{
		[Map]
		public virtual string Form { get; set; }
	}
}