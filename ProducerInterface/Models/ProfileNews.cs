using System;
using System.ComponentModel;
using Analit.Components;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;

namespace ProducerInterface.Models
{
	/// <summary>
	/// Модекль новости в профиле
	/// </summary>
	[Model(Database = "ProducerInterface", Table = "profile_news")]
	public class ProfileNews : BaseModel
	{
		public ProfileNews()
		{
			CreatedDate = SystemTime.Now();
		}

		[Map, Description("Заголовок"), ValidatorNotEmpty]
		public virtual string Topic { get; set; }

		[Map, Description("Содержание"), ValidatorNotEmpty]
		public virtual string Text { get; set; }

		[BelongsTo, Description("Кабинет поставщика"), ValidatorNotNull]
		public virtual Producer Producer { get; set; }

		[Map, Description("Дата редактирования")]
		public virtual DateTime EditedDate { get; set; }

		[Map, Description("Дата создания")]
		public virtual DateTime CreatedDate { get; set; }
	}
}