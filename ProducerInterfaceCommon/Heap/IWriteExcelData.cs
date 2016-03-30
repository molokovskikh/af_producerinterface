using OfficeOpenXml;
using ProducerInterfaceCommon.Models;
using System.Data;

namespace ProducerInterfaceCommon.Heap
{
	public interface IWriteExcelData
	{
		ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable, Report param);
	}
}


