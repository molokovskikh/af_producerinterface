using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using OfficeOpenXml;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.Models
{
	public class ProductRatingReportRow : ReportRow
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

		[Display(Name = "Упаковки, шт.")]
		public long? PosOrder { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля в % (упаковки)")]
		public decimal? PosOrderPercent { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Сумма, руб.")]
		public decimal? Summ { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля в % (рубли)")]
		public decimal? SummPercent { get; set; }

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

		[Display(Name = "Количество точек доставки")]
		public long? DistinctAddressId { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			var clist = list.Cast<ProductRatingReportRow>();
			var cparam = (ProductRatingReport)param;

			// фильтрация по производителю, если не выбрана опция "По всему ассортименту"
			if (cparam.Var != CatalogVar.AllAssortiment)
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

		public override void Format(ExcelWorksheet sheet)
		{
			sheet.Column(1).Width = 40;
			sheet.Column(2).Width = 30;
		}
	}
}