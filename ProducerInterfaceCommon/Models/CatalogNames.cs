using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class CatalogNames
	{
		public virtual uint Id { get; set; }
		public virtual string Name { get; set; }
		public virtual uint? DescriptionId { get; set; }
		public virtual uint? MnnId { get; set; }
		public virtual System.DateTime UpdateTime { get; set; }
	}
}