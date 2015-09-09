using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Model("Descriptions","catalogs")]
	public class DrugDescription : BaseModel
	{
		[Map]
		public virtual string Name { get; set; }

		[Map]
		public virtual string EnglishName { get; set; }

		[Map]
		public virtual string Description { get; set; }

		[Map]
		public virtual string Interaction { get; set; }

		[Map]
		public virtual string SideEffect { get; set; }

		[Map]
		public virtual string IndicationsForUse { get; set; }

		[Map]
		public virtual string Dosing { get; set; }

		[Map]
		public virtual string Warnings { get; set; }

		[Map]
		public virtual string ProductForm { get; set; }

		[Map]
		public virtual string PharmacologicalAction { get; set; }

		[Map]
		public virtual string Storage { get; set; }

		[Map]
		public virtual string Expiration { get; set; }

		[Map]
		public virtual string Composition { get; set; }

		[Map("NeedCorrect")]
		public virtual bool ShouldBeCorrected { get; set; }

		[Map]
		public virtual DateTime UpdateTime { get; set; }

	}
}