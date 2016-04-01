using OfficeOpenXml;
using ProducerInterfaceCommon.Heap;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProducerInterfaceCommon.Models
{
	public class ProductResidueReportRow : ReportRow, IWriteExcelData
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Производитель")]
		public string ProducerName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

		[Format(Value = "0.00")]
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
			var shredder = new ObjectShredder<ProductResidueReportRow>();
			var querySort = shredder.UnShred(dataTable);
			// вытащили различных поставщиков из набора. Сортировка - по количеству позиций в порядке убывания
			var suppliers = querySort.GroupBy(x => x.SupplierName)
				.Select(x => new { SupplierName = x.Key, PosCount = x.Sum(y => y.Quantity)})
				.OrderByDescending(x => x.PosCount).Select(x => x.SupplierName).ToArray(); 
			var dd = new Dictionary<string, int>();
			for (int i = 0; i < suppliers.Length; i++)
				dd.Add(suppliers[i], i + 1);

			var _type = typeof(ProductResidueReportRow);

			int startCol = 1;
			int startRow = dataStartRow;
			var keys = new List<string>() { "CatalogName", "ProducerName", "RegionName" };
			// записали ключи в имена колонок
			foreach (var p in _type.GetProperties().Where(x => keys.Contains(x.Name)))
			{
				// установили имя колонки
				var displayName = p.GetCustomAttribute<DisplayAttribute>().Name;
				ws.SetValue(startRow, startCol++, displayName);
			}

			// вытащили форматирование сумм
			var pCost = _type.GetProperty("Cost");
			var costFormat = pCost.GetCustomAttribute<FormatAttribute>().Value;

			// показываем цены, если есть цены
			var showCost = querySort.Any(x => x.Cost.HasValue);

			// вставили заголовки рассчитываемых колонок
			if (showCost) {
				ws.Column(startCol).Style.Numberformat.Format = costFormat;
				ws.SetValue(startRow, startCol++, "Мин. цена");

				ws.Column(startCol).Style.Numberformat.Format = costFormat;
				ws.SetValue(startRow, startCol++, "Средн. цена");

				ws.Column(startCol).Style.Numberformat.Format = costFormat;
				ws.SetValue(startRow, startCol++, "Макс. цена");
			}

			ws.SetValue(startRow, startCol++, "Лидер");

			// записали поставщиков в имена колонок
      foreach (var s in suppliers)
			{
				ws.SetValue(startRow - 1, startCol, s);
				if (showCost) {
					ws.Column(startCol).Style.Numberformat.Format = costFormat;
					ws.Cells[startRow - 1, startCol, startRow - 1, startCol + 1].Merge = true;
					ws.SetValue(startRow, startCol, "Цена");
					ws.SetValue(startRow, ++startCol, "Кол-во");
				}
				else {
					// мёрдж одной ячейки, чтобы была одинаковая ширина колонок вне зависимости от длины названия поставщика потому что слитые ячейки не участвуют в алгоритме автоопределения оптимальной ширины столбца
					ws.Cells[startRow - 1, startCol, startRow - 1, startCol].Merge = true;
					ws.SetValue(startRow, startCol, "Кол-во");
				}
				startCol++;
			}
			startRow++;
			var colCount = startCol - 1;

			// каждому key сопоставлен набор пар дата-цена
			var orderGrouping =
			from item in querySort
			let groupKey = new { item.CatalogName, item.ProducerName, item.RegionName }
			group new { item.SupplierName, item.Cost, item.Quantity } by groupKey into grouping
			orderby grouping.Key.CatalogName
			select grouping;

			// строка для каждого уникального ключа
      foreach (var group in orderGrouping)
			{
				startCol = 1;
				var key = group.Key;
				ws.SetValue(startRow, startCol++, key.CatalogName);
				ws.SetValue(startRow, startCol++, key.ProducerName);
				ws.SetValue(startRow, startCol++, key.RegionName);
	      if (showCost) {
		      ws.SetValue(startRow, startCol++, group.Min(x => x.Cost));
		      ws.SetValue(startRow, startCol++, group.Average(x => x.Cost));
		      ws.SetValue(startRow, startCol++, group.Max(x => x.Cost));
	      }

	      var maxQuantity = group.Max(x => x.Quantity);
				var liderName = group.Where(x => x.Quantity == maxQuantity).Select(x => x.SupplierName).FirstOrDefault();
        ws.SetValue(startRow, startCol, liderName);

				// записываем данные под соответствующим поставщиком
				foreach (var item in group)
				{
					if (showCost) {
						var col = startCol + 2 * dd[item.SupplierName] - 1;
						ws.SetValue(startRow, col, item.Cost);
						ws.SetValue(startRow, ++col, item.Quantity);
					}
					else {
						var col = startCol + dd[item.SupplierName];
						ws.SetValue(startRow, col, item.Quantity);
					}
				}
				++startRow;
			}
			var rowCount = startRow - 1;
			return new ExcelAddressBase(dataStartRow, 1, rowCount, colCount);
		}
	}
}