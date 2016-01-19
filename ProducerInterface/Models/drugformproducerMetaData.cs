using System.ComponentModel.DataAnnotations;

namespace ProducerInterface.Models
{
	[MetadataType(typeof(drugformproducerMetaData))]
	public partial class drugformproducer
	{
	}

	public class drugformproducerMetaData
	{
		[ScaffoldColumn(false)]
		public long CatalogId { get; set; }

		[ScaffoldColumn(false)]
		public long DrugFamilyId { get; set; }

		[ScaffoldColumn(false)]
		public long ProducerId { get; set; }

		[Display(Name = "Форма", Order = 10)]
		public string CatalogName { get; set; }

		[Display(Name = "Жизненно-важные", Order = 20)]
		public bool VitallyImportant { get; set; }

		[Display(Name = "Обязательный ассортимент", Order = 30)]
		public bool MandatoryList { get; set; }

		[Display(Name = "ПКУ: наркотические и психотропные", Order = 40)]
		public bool Narcotic { get; set; }

		[Display(Name = "ПКУ: сильнодействующие и ядовитые", Order = 50)]
		public bool Toxic { get; set; }

		[Display(Name = "ПКУ: комбинированные", Order = 60)]
		public bool Combined { get; set; }

		[Display(Name = "Монобренд", Order = 70)]
		public bool Monobrend { get; set; }
	}
}