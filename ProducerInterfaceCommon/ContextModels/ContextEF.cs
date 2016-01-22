using System.Linq;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System;

namespace ProducerInterfaceCommon.ContextModels
{
    public partial class producerinterface_Entities
    {

			public override int SaveChanges()
			{
			//foreach (ObjectStateEntry entry in (this as IObjectContextAdapter).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted))
			//{
			//	// Validate the objects in the Added and Modified state
			//	// if the validation fails throw an exeption.

			//	var vvve = entry.State;


			//}


			////this.ObjectContext = ((IObjectContextAdapter)this).ObjectContext;
			//// Get all Added/Deleted/Modified entities (not Unmodified or Detached)
			//foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Added || p.State == System.Data.Entity.EntityState.Deleted || p.State == System.Data.Entity.EntityState.Modified))
			//{
			//	// security session is not Auditable 
			//	var vvv = ent.State;


			//}
			var addedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Added).ToList();

			foreach (var change in addedEntities)
			{
				//var primaryKey = GetPrimaryKeyValue(change);
				// Get primary key value (If you have more than one key column, this will need to be adjusted)
				//var keyNames = change.Entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).ToList();

				//string keyName = keyNames[0].Name; //dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

			}


			var modifiedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Modified).ToList();



			var now = DateTime.UtcNow;

			foreach (var change in modifiedEntities)
			{
				var entityName = change.Entity.GetType().Name;
				var primaryKey = GetPrimaryKeyValue(change);

				foreach (var propName in change.OriginalValues.PropertyNames)
				{
					var originalValue = change.OriginalValues[propName];
					var currentValue = change.CurrentValues[propName];
					if (!object.Equals(currentValue, originalValue))
					{
						string cv = currentValue != null ? currentValue.ToString() : null;
						string ov = originalValue != null ? originalValue.ToString() : null;
					}
				}
			}

			var sdf = base.SaveChanges();
			return sdf;

			}

		private object GetPrimaryKeyValue(DbEntityEntry entry)
		{
			var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
			return objectStateEntry.EntityKey.EntityKeyValues[0].Value;
		}

		//private List<AuditLog> GetAuditRecordsForChange(DbEntityEntry dbEntry, int userId, Guid sessionId)
		//{
		//	// вытащили имя сущности = имя таблицы
		//	Type entityType = dbEntry.Entity.GetType();
		//	if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
		//		entityType = entityType.BaseType;

		//	string entityTypeName = entityType.Name;

		//	string[] keyNames;

		//	// вытащили имя идентификаторов
		//	MethodInfo method = Data.Helpers.EntityKeyHelper.Instance.GetType().GetMethod("GetKeyNames");
		//	keyNames = (string[])method.MakeGenericMethod(entityType).Invoke(Data.Helpers.EntityKeyHelper.Instance, new object[] {
		//				this
		//		});

		//	List<AuditLog> result = new List<AuditLog>();

		//	DateTime changeTime = DateTime.Now;

		//	// Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
		//	string tableName = entityTypeName;

		//	if (dbEntry.State == System.Data.Entity.EntityState.Added)
		//	{
		//		// For Inserts, just add the whole record
		//		// If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()

		//		foreach (string propertyName in dbEntry.CurrentValues.PropertyNames)
		//		{
		//			result.Add(new AuditLog()
		//			{
		//				AuditLogId = Guid.NewGuid(),
		//				UserId = userId,
		//				SessionId = sessionId,
		//				EventDateUTC = changeTime,
		//				EventType = "A", // Added
		//				TableName = tableName,
		//				RecordId = dbEntry.CurrentValues.GetValue<object>(keyNames[0]).ToString(),
		//				ColumnName = propertyName,
		//				NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null ? null : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString()
		//			});
		//		}
		//	}
		//	else if (dbEntry.State == System.Data.Entity.EntityState.Deleted)
		//	{
		//		// Same with deletes, do the whole record, and use either the description from Describe() or ToString()
		//		result.Add(new AuditLog()
		//		{
		//			AuditLogId = Guid.NewGuid(),
		//			UserId = userId,
		//			SessionId = sessionId,
		//			EventDateUTC = changeTime,
		//			EventType = "D", // Deleted
		//			TableName = tableName,
		//			RecordId = dbEntry.OriginalValues.GetValue<object>(keyNames[0]).ToString(),
		//			ColumnName = "*ALL",
		//			NewValue = (dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Describe() : dbEntry.OriginalValues.ToObject().ToString()
		//		});
		//	}
		//	else if (dbEntry.State == System.Data.Entity.EntityState.Modified)
		//	{
		//		foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
		//		{
		//			// For updates, we only want to capture the columns that actually changed
		//			if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
		//			{
		//				result.Add(new AuditLog()
		//				{
		//					AuditLogId = Guid.NewGuid(),
		//					UserId = userId,
		//					SessionId = sessionId,
		//					EventDateUTC = changeTime,
		//					EventType = "M", // Modified
		//					TableName = tableName,
		//					RecordId = dbEntry.OriginalValues.GetValue<object>(keyNames[0]).ToString(),
		//					ColumnName = propertyName,
		//					OriginalValue = dbEntry.OriginalValues.GetValue<object>(propertyName) == null ? null : dbEntry.OriginalValues.GetValue<object>(propertyName).ToString(),
		//					NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null ? null : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString()
		//				});
		//			}
		//		}
		//	}
		//	// Otherwise, don't do anything, we don't care about Unchanged or Detached entities

		//	return result;
		//}


	}
}
