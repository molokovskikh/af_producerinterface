using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

		[Hidden]
		[Display(Name = "Идентификатор производителя")]
		public long ProducerId { get; set; }

		//[Display(Name = "Поставщик")]
		//public string SupplierName { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			var clist = list.Cast<ProductRatingReportRow>();
			var cparam = (ProductRatingReport)param;

			// фильтрация по производителю, если не выбрана опция "По всему ассортименту"
			if (!cparam.AllAssortiment)
				clist = clist.Where(x => x.ProducerId == cparam.ProducerId).ToList();
			// сумма всего
			var sm = clist.Sum(x => x.Summ ?? 0);
			foreach (var item in clist)
			{
				if (!item.Summ.HasValue || sm == 0)
					continue;
				item.SummPercent = item.Summ.GetValueOrDefault() * 100 / sm;
			}

			// заказов всего
			var or = clist.Sum(x => x.PosOrder ?? 0);
			foreach (var item in clist)
			{
				if (!item.PosOrder.HasValue || or == 0)
					continue;
				item.PosOrderPercent = Convert.ToDecimal(item.PosOrder) * 100 / or;
			}

			return clist.Cast<T>().ToList();
		}
	}
}