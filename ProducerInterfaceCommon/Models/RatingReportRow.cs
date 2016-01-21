using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	public abstract class RatingReportRow : ReportRow
	{
		[Hidden]
		[Display(Name = "Сумма")]
		public decimal? Summ { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля рынка в %")]
		public decimal? SummPercent { get; set; }

		[Hidden]
		[Display(Name = "Заказ")]
		public long? PosOrder { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля от общего заказа в %")]
		public decimal? PosOrderPercent { get; set; }

		[Format(Value = "# ##0.00\"р.\";-# ##0.00\"р.\"")]
		[Round(Precision = 2)]
		[Display(Name = "Минимальная цена")]
		public decimal? MinCost { get; set; }

		[Format(Value = "# ##0.00\"р.\";-# ##0.00\"р.\"")]
		[Round(Precision = 2)]
		[Display(Name = "Средняя цена")]
		public decimal? AvgCost { get; set; }

		[Format(Value = "# ##0.00\"р.\";-# ##0.00\"р.\"")]
		[Round(Precision = 2)]
		[Display(Name = "Максимальная цена")]
		public decimal? MaxCost { get; set; }

		[Display(Name = "Кол-во заявок по препарату")]
		public long? DistinctOrderId { get; set; }

		[Display(Name = "Кол-во адресов доставки, заказавших препарат")]
		public long? DistinctAddressId { get; set; }

		public override List<T> Treatment<T>(List<T> list)
		{
			var clist = list.Cast<RatingReportRow>();
			// сумма всего
			var sm = clist.Sum(x => x.Summ ?? 0);
			foreach (var item in clist) {
				if (!item.Summ.HasValue || sm == 0)
					continue;
				item.SummPercent = item.Summ.GetValueOrDefault() * 100 / sm;
			}

			// заказов всего
			var or = clist.Sum(x => x.PosOrder ?? 0);
			foreach (var item in clist) {
				if (!item.PosOrder.HasValue || or == 0)
					continue;
				item.PosOrderPercent = Convert.ToDecimal(item.PosOrder) * 100 / or;
			}

			return list;
		}

	}
}