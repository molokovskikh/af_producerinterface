using OfficeOpenXml;
using OfficeOpenXml.Style;
using Quartz.Job.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace Quartz.Job
{
	public class ExcelCreator<T>
	{
		private Type _type;

		public ExcelCreator()
		{
			_type = typeof(T);
		}

		public void Create(FileInfo file, string sheetName, List<string> headers, DataTable dataTable)
		{
			using (var pck = new ExcelPackage(file)) {
				var ws = pck.Workbook.Worksheets.Add(sheetName);

				var dataStartRow = headers.Count + 2;
				
				// загрузили данные, оставив место для шапки
				ws.Cells[dataStartRow, 1].LoadFromDataTable(dataTable, true);

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
					// если колонка скрытая - не считаем
					var h = p.GetCustomAttribute<HiddenAttribute>();
					if (h == null)
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
	}
}
