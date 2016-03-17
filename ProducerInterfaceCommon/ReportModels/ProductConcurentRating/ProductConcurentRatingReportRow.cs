using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	class ProductConcurentRatingReportRow : RatingReportRow
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }
	}
}
