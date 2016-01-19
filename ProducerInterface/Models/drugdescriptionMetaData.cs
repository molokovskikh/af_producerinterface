using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterface.Models
{
	[MetadataType(typeof(drugdescriptionMetaData))]
	public partial class drugdescription
	{
	}

	public class drugdescriptionMetaData
	{
		[ScaffoldColumn(false)]
		public long? DescriptionId { get; set; }

		[Display(Name = "Наименование", Order = 10)]
		[UIHint("TextBox")]
		[StringLength(255, ErrorMessage = "Наименование препарата должно быть не длиннее 255 символов")]
		[Required(ErrorMessage = "Не указано наименование препарата")]
		public string DescriptionName { get; set; }

		[Display(Name = "Английское наименование", Order = 20)]
		[UIHint("TextBox")]
		[StringLength(255, ErrorMessage = "Наименование препарата должно быть не длиннее 255 символов")]
		public string EnglishName { get; set; }

		[Display(Name = "Формакологическое действие", Order = 30)]
		[UIHint("TextArea")]
		public string PharmacologicalAction { get; set; }

		[Display(Name = "Состав", Order = 40)]
		[UIHint("TextArea")]
		public string Composition { get; set; }

		[Display(Name = "Показания к применению", Order = 50)]
		[UIHint("TextArea")]
		public string IndicationsForUse { get; set; }

		[Display(Name = "Способ применения и дозы", Order = 60)]
		[UIHint("TextArea")]
		public string Dosing { get; set; }

		[Display(Name = "Взаимодействие", Order = 70)]
		[UIHint("TextArea")]
		public string Interaction { get; set; }

		[Display(Name = "Побочные эффекты", Order = 80)]
		[UIHint("TextArea")]
		public string SideEffect { get; set; }

		[Display(Name = "Предостережения и противопоказания", Order = 90)]
		[UIHint("TextArea")]
		public string Warnings { get; set; }

		[Display(Name = "Форма выпуска", Order = 100)]
		[UIHint("TextArea")]
		public string ProductForm { get; set; }

		[Display(Name = "Условия хранения", Order = 110)]
		[UIHint("TextArea")]
		public string Storage { get; set; }

		[Display(Name = "Срок годности", Order = 120)]
		[UIHint("TextArea")]
		public string Expiration { get; set; }

		[Display(Name = "Дополнительно", Order = 130)]
		[UIHint("TextArea")]
		public string Description { get; set; }
	}
}