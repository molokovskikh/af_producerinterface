﻿using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace ProducerInterfaceCommon.LoggerModels
{
	public class FrameLogger
	{

		private DbContext _context;
		private Account _user;
		private string _connString = ConfigurationManager.ConnectionStrings["producerinterface"].ConnectionString;
		private ObjectStateManager _objectStateManager;

		public FrameLogger(Account user, DbContext context)
		{
			_context = context;
			_user = user;
			_objectStateManager = ((IObjectContextAdapter)_context).ObjectContext.ObjectStateManager;
		}

		private string GetPrimaryKey(DbEntityEntry entry)
		{
			var objectStateEntry = _objectStateManager.GetObjectStateEntry(entry.Entity);
			return String.Join("_", objectStateEntry.EntityKey.EntityKeyValues.Select(x => x.Value));
		}

		public void Write(IEnumerable<DbEntityEntry> entries, string description)
		{

			// кто
			var set = new LogChangeSet() { UserId = _user.ID_LOG, Ip = _user.IP, Timestamp = DateTime.Now, Description = description };
			foreach (var entry in entries)
			{
				// вытащили имя сущности = имя таблицы
				var entityType = entry.Entity.GetType();
				if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
					entityType = entityType.BaseType;

				// какие объекты БД
				var obj = new LogObjectChange() { Action = (int)entry.State,
																					ObjectReference = entry.State == System.Data.Entity.EntityState.Added ? entry.Entity.GetHashCode().ToString() : GetPrimaryKey(entry),
																					TypeName = entityType.FullName };
				set.LogObjectChange.Add(obj);

				IEnumerable<string> propCollection;
				if (entry.State == System.Data.Entity.EntityState.Added)
					propCollection = entry.CurrentValues.PropertyNames;
				else
					propCollection = entry.OriginalValues.PropertyNames;

				foreach (var propName in propCollection)
				{
					// какие свойства объектов
					var prop = new LogPropertyChange() { PropertyName = propName };
					switch (entry.State)
					{
						case System.Data.Entity.EntityState.Added:
							var currentValue = entry.CurrentValues[propName];
							if (currentValue == null)
								continue;
							prop.ValueNew = currentValue.ToString();
							obj.LogPropertyChange.Add(prop);
							break;

						case System.Data.Entity.EntityState.Deleted:
							var originalValue = entry.OriginalValues[propName];
							// если свойство было null - пропускаем
							if (originalValue == null)
								continue;
							prop.ValueOld = originalValue.ToString();
							obj.LogPropertyChange.Add(prop);
							break;

						case System.Data.Entity.EntityState.Modified:
							var orValue = entry.OriginalValues[propName];
							var curValue = entry.CurrentValues[propName];
							// если свойство не изменилось - пропускаем
							if (object.Equals(curValue, orValue))
								continue;
							prop.ValueOld = orValue != null ? orValue.ToString() : null;
							prop.ValueNew = curValue != null ? curValue.ToString() : null;
              obj.LogPropertyChange.Add(prop);
              break;
					}
				}
			}
			var lcntx = new Logger_Entities();
			lcntx.LogChangeSet.Add(set);
			lcntx.SaveChanges();
		}

		public int LogThis(IEnumerable<DbEntityEntry> entries)
		{

			int logChangeSetId = 0;

			using (var conn = new MySqlConnection(_connString))
			{
				using (var comUser = new MySqlCommand("InsertLogChangeSet", conn))
				{
					// объявление и установка параметров пользователя
					comUser.CommandType = CommandType.StoredProcedure;
					comUser.Parameters.AddWithValue("@UserId", _user.ID_LOG);
					comUser.Parameters.AddWithValue("@Ip", _user.IP);
					comUser.Parameters.Add("@LogChangeSetId", MySqlDbType.Int32);
					comUser.Parameters["@LogChangeSetId"].Direction = ParameterDirection.Output;

					conn.Open();
					comUser.ExecuteNonQuery();
					logChangeSetId = (int)comUser.Parameters["@LogChangeSetId"].Value;
					using (var comObject = new MySqlCommand("InsertLogObjectChange", conn))
					{
						// объявление параметров объекта
						comObject.CommandType = CommandType.StoredProcedure;
						comObject.Parameters.Add("@TypeName", MySqlDbType.VarChar, 250); // varchar(250)
						comObject.Parameters.Add("@ObjectReference", MySqlDbType.VarChar, 250); // varchar(250)
						comObject.Parameters.Add("@Action", MySqlDbType.Int32); // int(10)
						comObject.Parameters.AddWithValue("@LogChangeSetId", logChangeSetId);
						comObject.Parameters.Add("@LogObjectChangeId", MySqlDbType.Int32);
						comObject.Parameters["@LogObjectChangeId"].Direction = ParameterDirection.Output;

						using (var comProp = new MySqlCommand("InsertLogPropertyChange", conn))
						{
							// объявление параметров свойств объекта
							comProp.CommandType = CommandType.StoredProcedure;
							comProp.Parameters.Add("@PropertyName", MySqlDbType.VarChar, 250); // varchar(250)
							comProp.Parameters.Add("@ValueOld", MySqlDbType.Text); // TEXT
							comProp.Parameters.Add("@ValueNew", MySqlDbType.Text); // TEXT
							comProp.Parameters.Add("@LogObjectChangeId", MySqlDbType.Int32);

							// для каждого объекта
							foreach (var entry in entries)
							{
								// вытащили имя сущности = имя таблицы
								var entityType = entry.Entity.GetType();
								if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
									entityType = entityType.BaseType;

								// установка параметров объекта
								comObject.Parameters["@TypeName"].Value = entityType.FullName;
								comObject.Parameters["@ObjectReference"].Value = entry.State == System.Data.Entity.EntityState.Added ? entry.Entity.GetHashCode().ToString() : GetPrimaryKey(entry);
								comObject.Parameters["@Action"].Value = (int)entry.State;

								comObject.ExecuteNonQuery();
								var logObjectChangeId = (int)comObject.Parameters["@LogObjectChangeId"].Value;
								comProp.Parameters["@LogObjectChangeId"].Value = logObjectChangeId;

								// для добавленных
								if (entry.State == System.Data.Entity.EntityState.Added)
									// для каждого свойства объекта
									foreach (var propName in entry.CurrentValues.PropertyNames)
									{
										var currentValue = entry.CurrentValues[propName];
										// не сохраняем значения null
										if (currentValue == null)
											continue;

										// установка параметров свойств объекта
										comProp.Parameters["@PropertyName"].Value = propName;
										comProp.Parameters["@ValueOld"].Value = null;
										comProp.Parameters["@ValueNew"].Value = currentValue.ToString();
										comProp.ExecuteNonQuery();
									}
								// для изменённых
								else if (entry.State == System.Data.Entity.EntityState.Modified)
									// для каждого свойства объекта
									foreach (var propName in entry.OriginalValues.PropertyNames)
									{
										var originalValue = entry.OriginalValues[propName];
										var currentValue = entry.CurrentValues[propName];
										// если свойство не изменилось - пропускаем
										if (object.Equals(currentValue, originalValue))
											continue;

										// установка параметров свойств объекта
										comProp.Parameters["@PropertyName"].Value = propName;
										comProp.Parameters["@ValueOld"].Value = originalValue != null ? originalValue.ToString() : null;
										comProp.Parameters["@ValueNew"].Value = currentValue != null ? currentValue.ToString() : null;
										comProp.ExecuteNonQuery();
									}
								// для удалённых
								else if (entry.State == System.Data.Entity.EntityState.Deleted)
									foreach (var propName in entry.OriginalValues.PropertyNames)
									{
										var originalValue = entry.OriginalValues[propName];
										// если свойство было null - пропускаем
										if (originalValue == null)
											continue;

										// установка параметров свойств объекта
										comProp.Parameters["@PropertyName"].Value = propName;
										comProp.Parameters["@ValueOld"].Value = originalValue.ToString();
										comProp.Parameters["@ValueNew"].Value = null;
										comProp.ExecuteNonQuery();
									}
							}
						}
					}
				}
			}
			return logChangeSetId;
		}
	}
}
