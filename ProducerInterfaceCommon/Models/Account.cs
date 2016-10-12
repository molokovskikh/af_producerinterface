using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class Account
	{
		public virtual uint Id { get; set; }
		public virtual string Login { get; set; }
	}
}