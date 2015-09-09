using System.Collections.Generic;
using System.ComponentModel;
using AnalitFramefork.Components;
using NHibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	///// <summary>
	///// Модель роли
	///// </summary>
	//[Class(0, Table = "roles", NameType = typeof(UserRole))]
	//public class UserRole : BaseModel
	//{
	//	[Property, Description("Наименование роли")]
	//	public virtual string Name { get; set; }

	//	[Property, Description("Описание роли")]
	//	public virtual string Description { get; set; }

	//	[Bag(0, Table = "perm_role", Cascade = "All", Lazy = CollectionLazy.False)]
	//	[Key(1, Column = "role", NotNull = false)]
	//	[ManyToMany(2, Column = "permission", ClassType = typeof(UserPermission))]
	//	public virtual IList<UserPermission> Permissions { get; set; }

	//	[Bag(0, Table = "user_role")]
	//	[Key(1, Column = "role", NotNull = false)]
	//	[ManyToMany(2, Column = "user", ClassType = typeof(User))]
	//	public virtual IList<User> Users { get; set; }

	//}
}