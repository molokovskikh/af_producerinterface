using System;
using System.ComponentModel;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Hibernate.Models;
using NHibernate;

namespace ProducerInterface.Models
{
	public enum DrugDescriptionRemarkStatus
	{
		[Description("Новая заявка")] New = 0,
		[Description("Примененная заявка")] Accepted = 1,
		[Description("Отклоненная заявка")] Declined = 2
	}

	[Model(Database = "producerInterface")]
	public class DrugDescriptionRemark : BaseModel
	{
		[Map]
		public virtual string Name { get; set; }

		[Map]
		public virtual string EnglishName { get; set; }

		[Map]
		public virtual string Description { get; set; }

		[Map]
		public virtual string Interaction { get; set; }

		[Map]
		public virtual string SideEffect { get; set; }

		[Map]
		public virtual string IndicationsForUse { get; set; }

		[Map]
		public virtual string Dosing { get; set; }

		[Map]
		public virtual string Warnings { get; set; }

		[Map]
		public virtual string ProductForm { get; set; }

		[Map]
		public virtual string PharmacologicalAction { get; set; }

		[Map]
		public virtual string Storage { get; set; }

		[Map]
		public virtual string Expiration { get; set; }

		[Map]
		public virtual string Composition { get; set; }

		[Map]
		public virtual DateTime CreationDate { get; set; }

		[Map]
		public virtual DrugDescriptionRemarkStatus Status { get; set; }

		[Map]
		public virtual DateTime ModificationDate { get; set; }

		[BelongsTo]
		public virtual Admin Modificator { get; set; }

		[BelongsTo]
		public virtual ProducerUser ProducerUser { get; set; }

		[BelongsTo]
		public virtual DrugFamily DrugFamily { get; set; }

		[BelongsTo]
		public virtual MNN MNN { get; set; }

		public DrugDescriptionRemark()
		{
			CreationDate = DateTime.Now;
			ModificationDate = DateTime.Now;
			Status = DrugDescriptionRemarkStatus.New;
		}

		public DrugDescriptionRemark(DrugFamily family) : this()
		{
			var description = family.DrugDescription;
			Name = description.Name;
			EnglishName = description.EnglishName;
			Description = description.Description;
			Interaction = description.Interaction;
			SideEffect = description.SideEffect;
			IndicationsForUse = description.IndicationsForUse;
			Dosing = description.Dosing;
			Warnings = description.Warnings;
			ProductForm = description.ProductForm;
			PharmacologicalAction = description.PharmacologicalAction;
			Storage = description.Storage;
			Expiration = description.Expiration;
			Composition = description.Composition;
			MNN = family.MNN;

			DrugFamily = family;
		}

		public virtual void Apply(ISession dbSession, Admin admin = null)
		{
			ModificationDate = DateTime.Now;
			Modificator = admin;
			Status = DrugDescriptionRemarkStatus.Accepted;

			
			var description = DrugFamily.DrugDescription;
			description.Name = Name;
			description.EnglishName = EnglishName;
			description.Description = Description;
			description.Interaction = Interaction;
			description.SideEffect = SideEffect;
			description.IndicationsForUse = IndicationsForUse;
			description.Dosing = Dosing;
			description.Warnings = Warnings;
			description.ProductForm = ProductForm;
			description.PharmacologicalAction = PharmacologicalAction;
			description.Storage = Storage;
			description.Expiration = Expiration;
			description.Composition = Composition;

			DrugFamily.MNN = MNN;
			dbSession.Save(this);
			dbSession.Save(DrugFamily);
		}

		public virtual void Decline(ISession dbSession, Admin admin = null)
		{
			ModificationDate = DateTime.Now;
			Modificator = admin;
			Status = DrugDescriptionRemarkStatus.Declined;
			dbSession.Save(this);
		}
    }
}