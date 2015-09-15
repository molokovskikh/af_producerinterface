using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Analit.Components;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Hibernate.Models;
using NHibernate;
using NHibernate.Mapping;

namespace ProducerInterface.Models
{
	[Model("promotions", "ProducerInterface")]
	public class Promotion : BaseModel
	{
		public Promotion()
		{
			RegionMask = 1;
			UpdateTime = SystemTime.Now();
			Begin = SystemTime.Now();
			End = SystemTime.Now();
			Drugs = new List<Drug>();
		}

		[Map, ValidatorNotEmpty]
		public virtual string Name { get; set; }

		[Map, ValidatorNotEmpty]
		public virtual string Annotation { get; set; }

		[Map]
		public virtual string PromoFile { get; set; }

		[Map]
		public virtual int RegionMask { get; set; }

		[Map]
		public virtual DateTime UpdateTime { get; set; }

		[Map]
		public virtual DateTime Begin { get; set; }

		[Map]
		public virtual DateTime End { get; set; }

		[Map]
		public virtual bool Status { get; set; }

		[Map]
		public virtual bool AgencyDisabled { get; set; }

		[Map]
		public virtual bool Enabled { get; set; }

		[BelongsTo]
		public virtual Admin Admin { get; set; }

		[BelongsTo, ValidatorNotNull]
		public virtual Producer Producer { get; set; }

		[BelongsTo, ValidatorNotNull]
		public virtual ProducerUser ProducerUser { get; set; }

		[HasMany(Table = "promotiontodrug", Database = "ProducerInterface", ManyToMany = true)]
		public virtual IList<Drug> Drugs { get; set; }


		public virtual void Apply(ISession dbSession, Admin admin)
		{
			UpdateTime = DateTime.Now;
			Admin = admin;
			Status = true;
			dbSession.Save(this);
		}

		public virtual void Decline(ISession dbSession, Admin admin)
		{
			UpdateTime = DateTime.Now;
			Admin = admin;
			Status = false;
			dbSession.Save(this);
		}
	}
}