using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Model("mnn","catalogs")]
	public class MNN : BaseModel
	{
		[Map("Mnn")]
		public virtual string Value { get; set; }

		[Map("RussianMnn")]
		public virtual string RussianValue { get; set; }

		[Map]
		public virtual DateTime UpdateTime { get; set; }
	}
}