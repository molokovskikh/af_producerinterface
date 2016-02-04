using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	public class ProductRatingReportRow : RatingReportRow
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

	}
}