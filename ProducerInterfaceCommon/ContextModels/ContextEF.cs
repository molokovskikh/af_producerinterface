using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ProducerInterfaceCommon.LoggerModels;

namespace ProducerInterfaceCommon.ContextModels
{
	public class Region
	{
		public ulong Id
		{
			get { return (ulong)RegionCode; }
			set { RegionCode = (long)value; }
		}
		public long RegionCode { get; set; }
		public string Name { get; set; }
	}

	public partial class producerinterface_Entities
	{
		public List<Region> Regions()
		{
			return Database.SqlQuery<Region>(@"select RegionCode, Region as Name
from Farm.Regions
where Parent is null and Retail = 0 and RegionCode > 0 and RegionCode <> 524288
order by Region").ToList();
		}

		public List<Region> Regions(ulong mask)
		{
			return Regions().Where(x => (x.Id & mask) > 0).ToList();
		}

		public int SaveChanges(Account user, string description = null)
		{
			var entries = ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
			var logger = new FrameLogger(user, this);
			logger.Write(entries, description);
			return base.SaveChanges();
		}
	}
}
