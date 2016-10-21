using System;
using System.Linq;
using Common.NHibernate;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using ProducerInterfaceCommon.Models;

namespace ProducerInterfaceCommon.ContextModels
{
	public class NHibernate : BaseNHibernate
	{
		public override void Init()
		{
			Mapper.Class<User>(x => x.Table("Account"));
			Mapper.Class<Job>(x => {
				x.Table("jobextend");
				x.Schema("ProducerInterface");
				x.ManyToOne(y => y.Owner, y => y.Column("CreatorId"));
				x.ManyToOne(y => y.Producer, y => y.Column("ProducerId"));
			});

			Mapper.Class<ProducerInterfaceCommon.Models.ServiceTaskManager>(x => {
				x.Table("ServiceTaskManager");
				x.Schema("ProducerInterface");
				x.ManyToOne(y => y.User, y => y.Column("AccountId"));
			});


			Mapper.Class<Producer>(x => {
				x.Table("Producers");
				x.Schema("Catalogs");
			});

			Mapper.Class<Catalog>(x => {
				x.Table("Catalog");
				x.Schema("Catalogs");
			});

			Mapper.Class<CatalogNames>(x => {
				x.Table("CatalogNames");
				x.Schema("Catalogs");
			});

			Mapper.Class<ProducerInterfaceCommon.Models.MediaFiles>(x => {
				x.Table("MediaFiles");
				x.Schema("ProducerInterface");
				x.Property(f => f.ImageFile, map => {
					map.Type(NHibernateUtil.BinaryBlob);
					map.Length(Int32.MaxValue);
				});
			});

			Mapper.Class<ProducerInterfaceCommon.Models.CatalogLog>(x => {
				x.Table("CatalogLog");
				x.Schema("ProducerInterface");
				x.Property(f => f.Before, map =>
				{
					map.Column("`Before`");
					map.NotNullable(false);
					map.Type(NHibernateUtil.StringClob);
				});
				x.Property(f => f.After, map => {
					map.Column("After");
					map.NotNullable(false);
					map.Type(NHibernateUtil.StringClob);
				});
			});

			Mapper.Class<ProducerInterfaceCommon.Models.Account>(x => {
				x.Table("Account");
				x.Schema("ProducerInterface");
			});

			Mapper.Class<DrugFormPicture>(x => {
				x.Table("DrugFormPictures");
				x.Schema("ProducerInterface");
			});

			Mapper.Class<Slide>(x => {
				x.Table("Slides");
				x.Schema("ProducerInterface");
			});

			Mapper.Class<MailFormWithFooter>(x => {
				x.Table("MailFormWithFooter");
				x.Schema("ProducerInterface");
			});

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