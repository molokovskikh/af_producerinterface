using OfficeOpenXml;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Models
{
	public class ProductPriceDynamicsReportRow : ReportRow, IWriteExcelData
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

		[Format(Value = "DD.MM.YYYY")]
		[Display(Name = "Дата")]
		public DateTime Date { get; set; }

		[Format(Value = "# ##0.00\"р.\";-# ##0.00\"р.\"")]
		[Round(Precision = 2)]
		[Display(Name = "Цена")]
		public decimal? Cost { get; set; }

		[Display(Name = "Количество")]
		public long? Quantity { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			return list;
		}

		public ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable, Report param)
		{
			// DataTable -> List
			var shredder = new ObjectShredder<ProductPriceDynamicsReportRow>();
			var querySort = shredder.UnShred(dataTable);
			// вытащили различные даты из набора
			var dates = querySort.Select(x => x.Date).Distinct().OrderBy(x => x).ToArray();
			var dd = new Dictionary<DateTime, int>();
			for (int i = 0; i < dates.Length; i++)
				dd.Add(dates[i], i + 1);

			var _type = typeof(ProductPriceDynamicsReportRow);

			int startCol = 1;
			int startRow = dataStartRow;
			var keys = new List<string>() { "CatalogName", "ProducerName", "RegionName", "SupplierName" };
			// записали ключи в имена колонок
			foreach (var p in _type.GetProperties().Where(x => keys.Contains(x.Name)))
			{
				// установили имя колонки
				var displayName = p.GetCustomAttribute<DisplayAttribute>().Name;
				ws.SetValue(startRow, startCol++, displayName);
			}

			// записали даты в имена колонок
			var pCost = _type.GetProperty("Cost");
			var costFormat = pCost.GetCustomAttribute<FormatAttribute>().Value;
			var cparam = (ProductPriceDynamicsReport)param;

			foreach (var d in dates) {
				ws.SetValue(startRow - 1, startCol, d.ToShortDateString());
				switch (cparam.VarCostOrQuantity) {
					case CostOrQuantity.WithCost:
						ws.Column(startCol).Style.Numberformat.Format = costFormat;
						ws.SetValue(startRow, startCol, "Цена");
						break;
					case CostOrQuantity.WithQuantity:
						ws.SetValue(startRow, startCol, "Кол-во");
						break;
					default:
						ws.Column(startCol).Style.Numberformat.Format = costFormat;
						ws.Cells[startRow - 1, startCol, startRow - 1, startCol + 1].Merge = true;
						ws.SetValue(startRow, startCol, "Цена");
						ws.SetValue(startRow, ++startCol, "Кол-во");
						break;
				}
				startCol++;
			}
			startRow++;
			var colCount = startCol - 1;

			// каждому key сопоставлен набор троек дата-цена-количество
			var orderGrouping =
			from item in querySort
			let groupKey = new { item.CatalogName, item.ProducerName, item.RegionName, item.SupplierName }
			group new { item.Date, item.Cost, item.Quantity } by groupKey into grouping
			//orderby grouping.Key.CatalogName
			select grouping;

			// строка для каждого уникального ключа
			foreach (var group in orderGrouping)
			{
				var key = group.Key;
				ws.SetValue(startRow, 1, key.CatalogName);
				ws.SetValue(startRow, 2, key.ProducerName);
				ws.SetValue(startRow, 3, key.RegionName);
				ws.SetValue(startRow, 4, key.SupplierName);
				// записываем данные под соответствующей датой
				foreach (var item in group) {
					int col;
					switch (cparam.VarCostOrQuantity)
					{
						case CostOrQuantity.WithCost:
							col = 4 + dd[item.Date];
							ws.SetValue(startRow, col, item.Cost);
							break;
						case CostOrQuantity.WithQuantity:
							col = 4 + dd[item.Date];
							ws.SetValue(startRow, col, item.Quantity);
							break;
						default:
							col = 4 + 2 * dd[item.Date] - 1;
							ws.SetValue(startRow, col, item.Cost);
							ws.SetValue(startRow, ++col, item.Quantity);
							break;
					}
				}
				++startRow;
			}
			var rowCount = startRow - 1;
			return new ExcelAddressBase(dataStartRow, 1, rowCount, colCount);
		}
	}
}
