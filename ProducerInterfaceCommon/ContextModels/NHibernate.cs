using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.NHibernate;
using MySql.Data.MySqlClient.Memcached;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using ProducerInterfaceCommon.Models;

namespace ProducerInterfaceCommon.ContextModels
{
	public class NHibernate : BaseNHibernate
	{
		public override void Init()
		{
			//Mapper.Class<Client>(x => x.Schema("Customers"));
			//Mapper.Class<User>(x => { x.Schema("Customers"); });
			//Mapper.Class<Outsider>(x => {
			//	x.Schema("Customers");
			//	x.Table("webftpoutsiders");
			//});
			//Mapper.Class<Admin>(x => {
			//	x.Schema("AccessRight");
			//	x.Table("RegionalAdmins");
			//	x.Id(y => y.Id, y => y.Column("RowId"));
			//	x.Property(o => o.Login, om => om.Column("UserName"));
			//	x.Property(o => o.Name, om => om.Column("ManagerName"));
			//});

			Mapper.Class<User>(x => x.Table("Account"));
			//Mapper.Class<Promotion>(x => {
			//	x.Table("Promotions");
			//	x.ManyToOne(y => y.MediaFile, y => y.ForeignKey("PromoFileId"));
			//	x.ManyToOne(y => y.Author, y => y.ForeignKey("ProducerUserId"));
			//	//x.Ignore(x => x.GlobalDrugList)
			//	//.Ignore(x => x.RegionnamesList)
			//	//.Ignore(x => x.RegionList)
			//	//.Ignore(x => x.DrugList);
			//});

			//var job = builder.Entity<Job>().ToTable("jobextend");
			//job.HasOptional(x => x.Owner).WithMany().Map(x => x.MapKey("CreatorId"));
			//job.HasOptional(x => x.Producer).WithMany().Map(x => x.MapKey("ProducerId"));
			Mapper.Class<Job>(x => {
				x.Table("jobextend");
				x.Schema("ProducerInterface");
				x.ManyToOne(y => y.Owner, y => y.Column("CreatorId"));
				x.ManyToOne(y => y.Producer, y => y.Column("ProducerId"));
			});

			Mapper.Class<Producer>(x => {
				x.Table("Producers");
				x.Schema("Catalogs");
			});

			//config.HasOptional(x => x.MediaFile).WithMany().Map(x => x.MapKey("PromoFileId"));
			//config.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("ProducerUserId"));
			//builder.Entity<PromotionsToSupplier>().ToTable("PromotionsToSupplier")
			//	.HasKey(x => new { x.PromotionId, x.SupplierId });
			//builder.Entity<PromotionToDrug>().ToTable("PromotionToDrug")
			//	.HasKey(x => new { x.PromotionId, x.DrugId });
			//builder.Entity<MediaFile>().ToTable("MediaFiles");

			//var snapshot = builder.Entity<PromotionSnapshot>().ToTable("PromotionSnapshots");
			//snapshot.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("AuthorId"));
			//snapshot.HasOptional(x => x.Promotion).WithMany().Map(x => x.MapKey("PromotionId"));
			//snapshot.HasOptional(x => x.File).WithMany().Map(x => x.MapKey("FileId"));

			//var email = builder.Entity<Email>().ToTable("mailform");
			//email.HasMany(x => x.MediaFiles).WithMany().Map(x => {
			//	x.ToTable("mailformToMediaFiles");
			//	x.MapLeftKey("MediaFilesId");
			//	x.MapRightKey("MailFormId");
			//});
			//var news = builder.Entity<News>().ToTable("Newses");
			////news.HasMany(x => x.NewsChange).WithOptional(x => x.News).Map(x => x.MapKey("NewsId"));
			//var newsSnapshot = builder.Entity<NewsSnapshot>().ToTable("NewsSnapshots");
			//newsSnapshot.HasOptional(x => x.Author).WithMany().Map(x => x.MapKey("AuthorId"));
			//newsSnapshot.HasOptional(x => x.News).WithMany(x => x.NewsChange).Map(x => x.MapKey("NewsId"));
			//

			var mapping = Mapper.CompileMappingForAllExplicitlyAddedEntities();

			var @class = Generators.Native.Class;
			foreach (var rootClass in mapping.RootClasses.Where(c => c.Id != null)) {
				if (rootClass.Id.generator == null) {
					rootClass.Id.generator = new HbmGenerator {
						@class = @class
					};
				}
			}
			Configuration.SetNamingStrategy(new PluralizeNamingStrategy());
			Configuration.AddDeserializedMapping(mapping, MappingAssembly.GetName().Name);
			Factory = Configuration.BuildSessionFactory();
		}
	}
}