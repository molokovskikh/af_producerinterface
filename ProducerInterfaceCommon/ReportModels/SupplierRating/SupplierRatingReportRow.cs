using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	public class SupplierRatingReportRow : RatingReportRow
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

	}
}