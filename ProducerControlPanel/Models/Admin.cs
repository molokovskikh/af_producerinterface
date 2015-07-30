using System;
using System.ComponentModel;
using AnalitFramefork.Components;
using NHibernate;
using NHibernate.Mapping.Attributes;
using NHibernate.Proxy;
using NHibernate.Validator.Engine;

namespace ProducerControlPanel.Models
{
	[Class(Table = "regionaladmins", NameType = typeof(Admin), Schema = "accessright")]
	public class Admin
	{
		/// <summary>
		/// Поле Id в данной таблицу RowID, иначе ругается, поэтому и BaseModel не наследуем
		/// </summary>
		/// 
		[Id(0, Name = "RowID")]
		[Generator(1, Class = "native")]
		public virtual int RowID { get; set; } 

		[Property(Name = "UserName"), Description("ФИО")]
		public virtual string UserName { get; set; }

		[Property(Name = "ManagerName"), Description("")]
		public virtual string ManagerName { get; set; }

		[Property(Name = "PhoneSupport"), Description("")]
		public virtual string PhoneSupport { get; set; }

		[Property(Name = "InternalPhone"), Description("")]
		public virtual string InternalPhone { get; set; }

		[Property(Name = "Email"), Description("")]
		public virtual string Email { get; set; }

		#region

		///TODO (для тестов!)  Не использовать 
		public virtual bool ChangeId(int newid, ISession session)
		{
			var attribute = Attribute.GetCustomAttribute(GetType(), typeof(ClassAttribute)) as ClassAttribute;
			var tablename = attribute.Table;
			var query = string.Format("UPDATE {0} SET id={1} WHERE id={2}", tablename, newid, RowID);
			session.CreateSQLQuery(query).ExecuteUpdate();
			session.Flush();
			return true;
		}

		public virtual Admin Unproxy()
		{
			var proxy = this as INHibernateProxy;
			if (proxy == null)
				return this;

			var session = proxy.HibernateLazyInitializer.Session;
			var model = (Admin)session.PersistenceContext.Unproxy(proxy);
			return model;
		}

		public virtual InvalidValue[] Validate(ISession session)
		{
			return new InvalidValue[0];
		}

		/// <summary>
		///  Получение имени таблицы
		/// </summary>
		/// <returns>имя таблицы</returns>
		public virtual string GetTableName()
		{
			var attribute = Attribute.GetCustomAttribute(this.GetType(), typeof(ClassAttribute)) as ClassAttribute;
			if (attribute != null) {
				return GetType().Name;
			}
			return "No name!";
		}

		#endregion
	}
}