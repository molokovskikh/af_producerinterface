using System.Collections.Generic;
using System.ComponentModel;
using AnalitFramefork.Components;
using NHibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	///// <summary>
	///// Модель прав
	///// </summary>
	//[Class(0, Table = "permissions", NameType = typeof(UserPermission))]
	//public class UserPermission : BaseModel
	//{
	//	[Property, Description("Наименование права")]
	//	public virtual string Name { get; set; }

	//	[Property, Description("Описание права")]
	//	public virtual string Description { get; set; }

	//	[Bag(0, Table = "perm_role", Lazy = CollectionLazy.False)]
	//	[Key(1, Column = "permission", NotNull = false)]
	//	[ManyToMany(2, Column = "role", ClassType = typeof(UserRole))]
	//	public virtual IList<UserRole> Roles { get; set; }

	//	[Bag(0, Table = "user_role", Lazy = CollectionLazy.False)]
	//	[Key(1, Column = "permission", NotNull = false)]
	//	[ManyToMany(2, Column = "user", ClassType = typeof(User))]
	//	public virtual IList<User> Users { get; set; }
	//}
}