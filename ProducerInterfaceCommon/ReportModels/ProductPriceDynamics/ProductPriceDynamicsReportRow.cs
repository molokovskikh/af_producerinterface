using OfficeOpenXml;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

		[Format(Value = "DD.MM.YYYY")]
		[Display(Name = "Дата")]
		public DateTime Date { get; set; }

		[Format(Value = "# ##0.00\"р.\";-# ##0.00\"р.\"")]
		[Round(Precision = 2)]
		[Display(Name = "Средняя цена")]
		public decimal? AvgCost { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			return list;
		}

		public ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable)
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
			var keys = new List<string>() { "CatalogName", "ProducerName", "RegionName" };
			// записали ключи в имена колонок
			foreach (var p in _type.GetProperties().Where(x => keys.Contains(x.Name)))
			{
				// установили имя колонки
				var displayName = p.GetCustomAttribute<DisplayAttribute>().Name;
				ws.SetValue(startRow, startCol++, (object)displayName);
			}

			// записали даты в имена колонок
			var pCost = _type.GetProperty("AvgCost");
			var costFormat = pCost.GetCustomAttribute<FormatAttribute>().Value;
			foreach (var d in dates) {
				ws.Column(startCol).Style.Numberformat.Format = costFormat;
				ws.SetValue(startRow, startCol++, d.ToShortDateString());
			}
			++startRow;
			var colCount = startCol - 1;

			// каждому key сопоставлен набор пар дата-цена
			var orderGrouping =
			from item in querySort
			let groupKey = new { item.CatalogName, item.ProducerName, item.RegionName }
			group new { item.Date, item.AvgCost } by groupKey into grouping
			orderby grouping.Key.CatalogName
			select grouping;

			// строка для каждого уникального ключа
			foreach (var group in orderGrouping)
			{
				var key = group.Key;
				ws.SetValue(startRow, 1, (object)key.CatalogName);
				ws.SetValue(startRow, 2, (object)key.ProducerName);
				ws.SetValue(startRow, 3, (object)key.RegionName);
				// записываем данные под соответствующей датой
				foreach (var item in group)
				{
					var col = 3 + dd[item.Date];
					ws.SetValue(startRow, col, (object)item.AvgCost);
				}
				++startRow;
			}
			var rowCount = startRow - 1;
			return new ExcelAddressBase(dataStartRow, 1, rowCount, colCount);
		}
	}
}
