using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.CatalogModels
{
	[MetadataType(typeof(mnnMetaData))]
	[DisplayName("МНН")]
	public partial class mnn
	{
		
	}

	public class mnnMetaData
	{
		[HiddenInput(DisplayValue = false)]
		public long Id { get; set; }

		[Display(Name = "Международное непатентованное наименование")]
		public string Mnn1 { get; set; }

		[Display(Name = "Международное непатентованное наименование (рус.)")]
		public string RussianMnn { get; set; }

		[ScaffoldColumn(false)]
		public DateTime UpdateTime { get; set; }

	}



	[MetadataType(typeof(CatalogMetaData))]
	[DisplayName("Каталог")]
	public partial class Catalog
	{
	}

	public class CatalogMetaData
	{

		[Display(Name = "Наименование", Order = 10)]
		public string Name { get; set; }

		[Display(Name = "Жизненно важные", Order = 20)]
		public bool VitallyImportant { get; set; }

		[Display(Name = "Обязательный ассортимент", Order = 30)]
		public bool MandatoryList { get; set; }

		[Display(Name = "ПКУ: наркотические и психотропные", Order = 40)]
		public bool Narcotic { get; set; }

		[Display(Name = "ПКУ: сильнодействующие и ядовитые", Order = 50)]
		public bool Toxic { get; set; }

		[Display(Name = "ПКУ: иные лекарственные средства", Order = 60)]
		public bool Other { get; set; }

		[Display(Name = "ПКУ: комбинированные", Order = 70)]
		public bool Combined { get; set; }

		[Display(Name = "Данный препарат с указанной формой выпуска и дозировкой производится исключительно ", Order = 80)]
		public bool Monobrend { get; set; }

		[Display(Name = "Время последней корректировки информации", Order = 90)]
		public DateTime UpdateTime { get; set; }

	}

	[MetadataType(typeof(DescriptionsMetaData))]
	[DisplayName("Описание препарата")]
	public partial class Descriptions
	{
	}

	public class DescriptionsMetaData
	{
		[HiddenInput(DisplayValue = false)]
		public long Id { get; set; }

		[Display(Name = "Наименование", Order = 10)]
		[UIHint("TextBox")]
		[Required]
		public string Name { get; set; }

		[Display(Name = "Английское наименование", Order = 20)]
		[UIHint("TextBox")]
		public string EnglishName { get; set; }

		[Display(Name = "Фармакологическое действие", Order = 30)]
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

		[ScaffoldColumn(false)]
		public bool NeedCorrect { get; set; }

		[ScaffoldColumn(false)]
		public DateTime UpdateTime { get; set; }
	}
}
