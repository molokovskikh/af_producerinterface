using System.Collections.Generic;
using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	/// <summary>
	/// Модель производителя TODO:добавить недостающие поля
	/// </summary>
	[Model(Database = "catalogs", Table = "Producers")]
	public class Producer : BaseModel
	{
		public Producer()
		{
			Drugs = new List<Drug>();
			//ProducerUsers = new List<ProducerUser>();
		}

		[Map, Description("Название"), ValidatorNotEmpty]
		public virtual string Name { get; set; } 

		[HasMany(ManyToMany = true, Table= "assortment", Column = "CatalogId")]
		public virtual IList<Drug> Drugs { get; set; }

		//[HasMany(ManyToMany = true, Table = "ProducerUser", Database = "ProducerInterface", Column = "ProducerId")]
		//public virtual IList<ProducerUser> ProducerUsers { get; set; }
	}
}