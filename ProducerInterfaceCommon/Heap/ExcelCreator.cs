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
				ExcelAddressBase dataAddress;

				// загрузили данные, оставив место для шапки
				// если определён загрузчик
				if (_type.GetInterfaces().Contains(typeof(IWriteExcelData))) {
					var instance = (IWriteExcelData)Activator.CreateInstance(_type);
					dataAddress = instance.WriteExcelData(ws, dataStartRow, dataTable);
				}
				// иначе - общий
				else {
					dataAddress = WriteExcelData(ws, dataStartRow, dataTable);
				}

				// установили автофильтры
				ws.Cells[dataAddress.Start.Row, 1, dataAddress.Start.Row, dataAddress.End.Column].AutoFilter = true;

				// установили рамку
        var border = ws.Cells[dataAddress.Address].Style.Border;
				border.Top.Style = border.Left.Style = border.Right.Style = border.Bottom.Style = ExcelBorderStyle.Thin;

				var da = ws.Cells[ws.Dimension.Address];
				// установили размер шрифта
				da.Style.Font.Name = "Arial Narrow";
				da.Style.Font.Size = 8;
				// установили ширину колонок
				da.AutoFitColumns();

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

		private ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable)
		{
			// установили форматы для открытых колонок, скрытые удалили
			int j = 1;
			foreach (var p in _pi)
			{
				if (!Attribute.IsDefined(p, typeof(HiddenAttribute))) {
					var f = p.GetCustomAttribute<FormatAttribute>();
					if (f != null)
						ws.Column(j).Style.Numberformat.Format = f.Value;
					j++;
				}
				else {
					dataTable.Columns.Remove(p.Name);
        }
			}
			ws.Cells[dataStartRow, 1].LoadFromDataTable(dataTable, true);

			// диапазон, занимаемый данными
			return new ExcelAddressBase(dataStartRow, 1, dataStartRow + dataTable.Rows.Count, dataTable.Columns.Count);
		}
	}
}
