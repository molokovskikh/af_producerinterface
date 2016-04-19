using System.Linq;
using System.Data.Entity;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.LoggerModels;

namespace ProducerInterfaceCommon.CatalogModels
{
	public partial class catalogsEntities
	{
		public int SaveChanges(Account user, string description = null)
		{
			var entries = this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
			var logger = new FrameLogger(user, this);
			logger.Write(entries, description);
			return base.SaveChanges();
		}
	}
}
