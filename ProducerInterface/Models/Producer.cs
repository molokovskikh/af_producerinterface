using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	/// <summary>
	/// Модель производителя TODO:добавить недостающие поля
	/// </summary>
	[Model(Database = "ProducerInterface", Table = "Producers")]
	public class Producer : BaseModel
	{
		[Map, Description("Название"), ValidatorNotEmpty]
		public virtual string Name { get; set; }
	}
}