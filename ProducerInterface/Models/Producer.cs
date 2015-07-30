using System.ComponentModel;
using AnalitFramefork.Components;
using NHibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	[Class(Table = "Producers", NameType = typeof(Producer), Schema = "producerinterface")]
	public class Producer : BaseModel
	{
		[Property, Description("Название")]
		public virtual string Name { get; set; }
	}
}