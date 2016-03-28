using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class ProductResidueReportRow : ReportRow
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

		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Цена")]
		public decimal? Summ { get; set; }

		[Display(Name = "Количество")]
		public long? PosOrder { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			var clist = list.Cast<ProductResidueReportRow>();
			var cparam = (ProductRatingReport)param;

			// фильтрация по производителю, если не выбрана опция "По всему ассортименту"
			if (cparam.Var != CatalogVar.AllAssortiment)
				clist = clist.Where(x => x.ProducerId == cparam.ProducerId).ToList();

			return clist.Cast<T>().ToList();
		}
	}
}