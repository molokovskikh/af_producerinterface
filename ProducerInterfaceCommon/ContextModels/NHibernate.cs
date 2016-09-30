using System;
using System.Linq;
using Common.NHibernate;
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

			Mapper.Class<Producer>(x => {
				x.Table("Producers");
				x.Schema("Catalogs");
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