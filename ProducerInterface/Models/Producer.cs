using System.ComponentModel;
using AnalitFramefork.Components;
using NHibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Class(0, Table = "Producers", NameType = typeof(Producer))]
	public class Producer : BaseModel
	{
		[Property, Description("Название")]
		public virtual string Name { get; set; }
	}
}