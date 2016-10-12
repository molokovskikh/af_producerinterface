using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class Catalog
	{
		public virtual uint Id { get; set; }
		public virtual uint FormId { get; set; }
		public virtual uint NameId { get; set; }
		public virtual bool VitallyImportant { get; set; }
		public virtual bool MandatoryList { get; set; }
		public virtual bool NeedCold { get; set; }
		public virtual bool Fragile { get; set; }
		public virtual bool Pharmacie { get; set; }
		public virtual bool Hidden { get; set; }
		public virtual string Name { get; set; }
		public virtual System.DateTime UpdateTime { get; set; }
		public virtual bool Monobrend { get; set; }
		public virtual bool Narcotic { get; set; }
		public virtual bool Toxic { get; set; }
		public virtual bool Combined { get; set; }
		public virtual bool Other { get; set; }
	}
}