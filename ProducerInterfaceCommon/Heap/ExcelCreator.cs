using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProducerInterfaceCommon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ProducerInterfaceCommon.Heap
{
	public class ExcelCreator<T>
	{
		private Type _type;
		private PropertyInfo[] _pi;

		public ExcelCreator()
		{
			_type = typeof(T);
			_pi = _type.GetProperties();
		}

		public void Create(FileInfo file, string sheetName, List<string> headers, DataTable dataTable)
		{
			using (var pck = new ExcelPackage(file)) {
				var ws = pck.Workbook.Worksheets.Add(sheetName);

				var dataStartRow = headers.Count + 2;
				
				// загрузили данные, оставив место для шапки
				//ws.Cells[dataStartRow, 1].LoadFromDataTable(dataTable, true);

				var itemList = UnShred(dataTable);

				//var clist = itemList.Cast<ProductPriceDynamicsReportRow>();
				//var dates = clist.Select(x => x.Date).Distinct().OrderBy(x => x).ToArray();
				//var dd = new Dictionary<DateTime, int>();
				//for (int i = 0; i < dates.Length; i++) {
				//	dd.Add(dates[i], i+1);
				//}
				//var t = typeof(ProductPriceDynamicsReportRow);
				//var pp = t.GetProperty("Date");

				//var orderGrouping =
				//from item in clist
				//let groupKey = new { item.CatalogName, item.ProducerName, item.RegionName }
				//group new { item.Date, item.AvgCost } by groupKey into grouping
				//orderby grouping.Key
				//select grouping;

				//int startCol = 1;
				//int startRow = dataStartRow;
				//foreach (var group in orderGrouping) {
				//	var key = group.Key;
				//	ws.SetValue(startRow, 1, (object)key.CatalogName);
				//	ws.SetValue(startRow, 2, (object)key.ProducerName);
				//	ws.SetValue(startRow, 3, (object)key.RegionName);
				//	foreach (var item in group) {
				//		var col = 3 + dd[item.Date];
				//		ws.SetValue(startRow, col, (object)item.AvgCost);
				//	}
				//	++startRow;
				//}

				int startCol = 1;
				int startRow = dataStartRow;
				foreach (PropertyInfo p in _pi)
				{
					if (!Attribute.IsDefined(p, typeof(HiddenAttribute)))
					{
						var displayName = p.GetCustomAttribute<DisplayAttribute>().Name;
						ws.SetValue(startRow, startCol++, (object)displayName);
					}
				}
				++startRow;

				foreach (var item in itemList)
				{
					startCol = 1;
					foreach (PropertyInfo p in _pi)
					{
						if (!Attribute.IsDefined(p, typeof(HiddenAttribute)))
						{
							ws.SetValue(startRow, startCol++, p.GetValue(item));
						}
					}
					++startRow;
				}

				// установили автофильтры
				ws.Cells[dataStartRow, 1, dataStartRow, dataTable.Columns.Count].AutoFilter = true;

				// установили рамку
				var border = ws.Cells[dataStartRow, 1, dataStartRow + dataTable.Rows.Count, dataTable.Columns.Count].Style.Border;
				border.Top.Style = border.Left.Style = border.Right.Style = border.Bottom.Style = ExcelBorderStyle.Thin;

				var da = ws.Cells[ws.Dimension.Address];
				// установили размер шрифта
				da.Style.Font.Name = "Arial Narrow";
				da.Style.Font.Size = 8;
				// установили ширину колонок
				da.AutoFitColumns();

				// установили форматы
				int j = 1;
				foreach (var p in _type.GetProperties()) {
					var f = p.GetCustomAttribute<FormatAttribute>();
					if (f != null)
						ws.Column(j).Style.Numberformat.Format = f.Value;
					//// если колонка скрытая - не считаем
					//var h = p.GetCustomAttribute<HiddenAttribute>();
					//if (h == null)
					j++;
				}

				// добавили шапку
				for (int i = 0; i < headers.Count; i++) {
					var er = ws.Cells[i + 1, 1];
					er.Value = headers[i];
					er.Style.Font.Name = "Arial Narrow";
					er.Style.Font.Size = 8;
				}

				pck.Save();
			}
		}

		private List<T> UnShred(DataTable table)
		{
			List<T> result = new List<T>();
			foreach (DataRow dr in table.Rows)
				result.Add(Conversion(dr));

			return result;
		}

		private T Conversion(DataRow dr)
		{
			var instance = (T)Activator.CreateInstance(_type);
			foreach (PropertyInfo p in _pi)
			{
				var f = dr[p.Name];
				if (f != null)
					p.SetValue(instance, Convert.ChangeType(f, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType));
			}
			return instance;
		}

	}
}
