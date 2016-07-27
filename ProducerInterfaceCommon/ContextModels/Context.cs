using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

	public class Producer
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class Job
	{
		public virtual int Id { get; set; }
		[Display(Name = "Формировать отчет")]
		public virtual string SchedName { get; set; }
		public virtual string JobName { get; set; }
		public virtual string JobGroup { get; set; }
		[Display(Name = "Название")]
		public virtual string CustomName { get; set; }
		public virtual string Scheduler { get; set; }
		[Display(Name = "Тип и параметры")]
		public virtual Reports ReportType { get; set; }
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate { get; set; }
		[Display(Name = "Изменен")]
		public virtual DateTime LastModified { get; set; }
		[Display(Name = "Статус")]
		public virtual DisplayStatus DisplayStatus { get; set; }
		[Display(Name = "Запуск")]
		public virtual DateTime? LastRun { get; set; }
		public virtual bool Enable { get; set; }

		[Display(Name = "Производитель")]
		public virtual Producer Producer { get; set; }
		[Display(Name = "Создатель")]
		public virtual User Owner { get; set; }
	}

	public class Email
	{
		public int Id { get; set; }
		public virtual string Subject { get; set; }
		public virtual string Body { get; set; }
		public virtual bool IsBodyHtml { get; set; }
		public virtual string Description { get; set; }

		public virtual ICollection<MediaFile> MediaFiles { get; set; }
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
		public DbSet<PromotionSnapshot> PromotionHistory { get; set; }
		public DbSet<Email> Emails { get; set; }
		public DbSet<News> Newses { get; set; }
		public DbSet<NewsSnapshot> NewsHistory { get; set; }

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

			var snapshot = builder.Entity<PromotionSnapshot>().ToTable("PromotionSnapshots");
			snapshot.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("AuthorId"));
			snapshot.HasOptional(x => x.Promotion).WithMany().Map(x => x.MapKey("PromotionId"));
			snapshot.HasOptional(x => x.File).WithMany().Map(x => x.MapKey("FileId"));

			var email = builder.Entity<Email>().ToTable("mailform");
			email.HasMany(x => x.MediaFiles).WithMany().Map(x => {
				x.ToTable("mailformToMediaFiles");
				x.MapLeftKey("MediaFilesId");
				x.MapRightKey("MailFormId");
			});
			builder.Entity<News>().ToTable("Newses");
			var newsSnapshot = builder.Entity<NewsSnapshot>().ToTable("NewsSnapshots");
			newsSnapshot.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("AuthorId"));
			newsSnapshot.HasOptional(x => x.News).WithMany(x => x.NewsChange).Map(x => x.MapKey("NewsId"));
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
