using System;
using NHibernate;
using NHibernate.Mapping.Attributes;
using NHibernate.Proxy;
using NHibernate.Validator.Engine;

namespace AnalitFramefork.Components
{
	/// <summary>
	/// Базовая модель, от которой все наследуется
	/// </summary>
	public class BaseModel
	{
		[Id(0, Name = "Id")]
		[Generator(1, Class = "native")]
		public virtual int Id { get; set; }

		
		///TODO (для тестов!)  Не использовать 
		public virtual bool ChangeId(int newid, ISession session)
		{
			var attribute = Attribute.GetCustomAttribute(GetType(), typeof(ClassAttribute)) as ClassAttribute;
			var tablename = attribute.Table;
			var query = string.Format("UPDATE {0} SET id={1} WHERE id={2}", tablename, newid, Id);
			session.CreateSQLQuery(query).ExecuteUpdate();
			session.Flush();
			return true;
		}

		public virtual BaseModel Unproxy()
		{
			var proxy = this as INHibernateProxy;
			if (proxy == null)
				return this;

			var session = proxy.HibernateLazyInitializer.Session;
			var model = (BaseModel)session.PersistenceContext.Unproxy(proxy);
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
	}
}