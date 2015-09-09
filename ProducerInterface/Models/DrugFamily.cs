using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Model("catalogNames","catalogs")]
	public class DrugFamily : BaseModel
	{
		[Map]
		public virtual string Name { get; set; }

		[BelongsTo("DescriptionId")]
		public virtual DrugDescription DrugDescription { get; set; }

		[BelongsTo]
		public virtual MNN MNN { get; set; }

		[Map]
		public virtual DateTime UpdateTime { get; set; }
	}
}