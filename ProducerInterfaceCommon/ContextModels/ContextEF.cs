using System.Linq;
using System.Data.Entity;
using System.Data;
using ProducerInterfaceCommon.LoggerModels;

namespace ProducerInterfaceCommon.ContextModels
{
	public partial class producerinterface_Entities
	{

		public int SaveChanges(Account user, string description = null)
		{
			var entries = this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
			var logger = new FrameLogger(user, this);
			logger.Write(entries, description);
			return base.SaveChanges();
		}

		//foreach (ObjectStateEntry entry in (this as IObjectContextAdapter).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted))

		//private List<AuditLog> GetAuditRecordsForChange(DbEntityEntry dbEntry, int userId, Guid sessionId)
		//{
		//	// вытащили имя идентификаторов
		//	MethodInfo method = Data.Helpers.EntityKeyHelper.Instance.GetType().GetMethod("GetKeyNames");
		//	keyNames = (string[])method.MakeGenericMethod(entityType).Invoke(Data.Helpers.EntityKeyHelper.Instance, new object[] {
		//				this
		//		});


		//		dbEntry.OriginalValues.GetValue<object>(propertyName)
		// If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()



	}
}
