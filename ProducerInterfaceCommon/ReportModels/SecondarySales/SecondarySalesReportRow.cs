using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	public class SecondarySalesReportRow : ReportRow
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Сумма")]
		public decimal? Summ { get; set; }

		[Display(Name = "Упаковки")]
		public long? PosOrder { get; set; }

		[Display(Name = "Кол-во заявок по препарату")]
		public long? DistinctOrderId { get; set; }

		[Display(Name = "Количество точек доставки")]
		public long? DistinctAddressId { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			return list;
		}
	}
}