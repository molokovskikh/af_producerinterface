using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class CatalogLog
	{
		public virtual uint Id { get; set; }
		public virtual uint NameId { get; set; }
		public virtual DateTime LogTime { get; set; }
		public virtual uint UserId { get; set; }
		public virtual string OperatorHost { get; set; }
		public virtual uint ObjectReference { get; set; }
		public virtual string ObjectReferenceNameUi { get; set; }
		public virtual int Type { get; set; }
		public virtual string PropertyName { get; set; }
		public virtual string PropertyNameUi { get; set; }
		public virtual string Before { get; set; }
		public virtual string After { get; set; }
		public virtual int Apply { get; set; }
		public virtual uint? AdminId { get; set; }
		public virtual DateTime? DateEdit { get; set; }
		public virtual uint? ProducerId { get; set; }
	}
}