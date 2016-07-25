using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using OfficeOpenXml;
using ProducerInterfaceCommon.Heap;

namespace ProducerInterfaceCommon.Models
{
	public abstract class ReportRow : IWriteExcelData
	{
		public abstract List<T> Treatment<T>(List<T> list, Report param) where T : ReportRow;

		public virtual ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable, Report param)
		{
			var type = GetType();
			var properties = type.GetProperties();
			int j = 1;
			foreach (var p in properties)
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

		public virtual void Format(ExcelWorksheet sheet)
		{
		}
	}
}
