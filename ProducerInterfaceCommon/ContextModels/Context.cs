using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ProducerInterfaceCommon.LoggerModels;
using ProducerInterfaceCommon.Models;

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

	public class Context : DbContext
	{
		public Context() : base("db")
		{
		}

		public DbSet<Promotion> Promotions { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<PromotionToDrug> PromotionToDrugs { get; set; }
		public DbSet<PromotionsToSupplier> PromotionsToSuppliers { get; set; }
		public DbSet<MediaFile> MediaFiles { get; set; }

		protected override void OnModelCreating(DbModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<User>().ToTable("Account");
			var config = builder.Entity<Promotion>()
				.ToTable("Promotions")
				.Ignore(x => x.GlobalDrugList)
				.Ignore(x => x.RegionnamesList)
				.Ignore(x => x.RegionList)
				.Ignore(x => x.DrugList);
			config.HasOptional(x => x.MediaFile).WithMany().Map(x => x.MapKey("PromoFileId"));
			config.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("ProducerUserId"));
			builder.Entity<PromotionsToSupplier>().ToTable("PromotionsToSupplier")
				.HasKey(x => new { x.PromotionId, x.SupplierId });
			builder.Entity<PromotionToDrug>().ToTable("PromotionToDrug")
				.HasKey(x => new { x.PromotionId, x.DrugId });
			builder.Entity<MediaFile>().ToTable("MediaFiles");
		}
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

		protected override void OnModelCreating(DbModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Promotion>().ToTable("Promotions");
			builder.Entity<PromotionsToSupplier>().ToTable("PromotionsToSupplier");
			builder.Entity<PromotionToDrug>().ToTable("PromotionToDrug");
		}
	}
}
