using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProducerInterfaceCommon.Heap
{
	public class HeaderHelper
	{
		private producerinterface_Entities _cntx;

		public HeaderHelper()
		{
			_cntx = new producerinterface_Entities();
		}

		public string GetDateHeader(DateTime dateFrom, DateTime dateTo)
		{
			return $"Период дат: {dateFrom} - {dateTo}";
		}

		public string GetDateHeader(DateTime dateFrom)
		{
			return $"Отчет создан на дату: {dateFrom.ToShortDateString()}";
		}

		public string GetRegionHeader(List<decimal> regionCodes)
		{
			var regions = _cntx.Regions().Where(x => regionCodes.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList();
			return $"В отчет включены следующие регионы: {String.Join(", ", regions)}";
		}

		public string GetProductHeader(List<long> productIds)
		{
			var products = _cntx.catalognames.Where(x => productIds.Contains(x.CatalogId)).Select(x => x.CatalogName).OrderBy(x => x).ToList();
			return $"В отчет включены следующие препараты: {String.Join(", ", products)}";
		}

		public string GetNotSupplierHeader(List<long> supplierIds)
		{
			var suppliers = _cntx.suppliernames.Where(x => supplierIds.Contains(x.SupplierId)).Select(x => x.SupplierName).OrderBy(x => x).ToList();
			return $"Из отчета исключены следующие поставщики: {String.Join(", ", suppliers)}";
		}

		public string GetSupplierHeader(List<long> supplierIds)
		{
			var suppliers = _cntx.suppliernames.Where(x => supplierIds.Contains(x.SupplierId)).Select(x => x.SupplierName).OrderBy(x => x).ToList();
			return $"В отчет включены следующие поставщики: {String.Join(", ", suppliers)}";
		}
	}
}
