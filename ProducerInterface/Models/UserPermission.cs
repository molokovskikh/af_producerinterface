using System.Collections.Generic;
using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	/// <summary>
	/// Модель прав пользователя
	/// </summary>
	[Model(Database = "ProducerInterface")]
	public class UserPermission : BaseModel
	{
		[Map, Description("Наименование права")]
		public virtual string Name { get; set; }

		[Map, Description("Описание права")]
		public virtual string Description { get; set; }

		[HasMany(Table = "usertouserrole", ManyToMany = true)]
		public virtual IList<ProducerUser> Users { get; set; }
	}
}