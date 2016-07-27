using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Tools;
using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class SecondarySalesReport : IntervalReport
	{
		public override string Name => "Продажи вторичных дистрибьюторов";

		[Display(Name = "Регион")]
		[CollectionRequired(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("Products")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "По всем нашим товарам")]
		[UIHint("ProductsFlag")]
		public bool AllCatalog { get; set; }

		public SecondarySalesReport()
		{
			AllCatalog = true;
			RegionCodeEqual = new List<decimal>();
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string> {
				h.GetDateHeader(DateFrom, DateTo),
				h.GetRegionHeader(RegionCodeEqual),
				GetCatalogHeader(h, AllCatalog, CatalogIdEqual)
			};
			return result;
		}

		public override MySqlCommand GetCmd(MySqlConnection connection)
		{
			var filter = "";
			var join = "";
			if (AllCatalog) {
				if (ProducerId != null)
					join = $"join Catalogs.Assortment a on a.CatalogId = CatalogId and a.ProducerId = {ProducerId}";
			} else {
				filter += $" and CatalogId in {CatalogIdEqual.Implode()}";
			}
			if (ProducerId != null)
				filter += $" and ProducerId = {ProducerId}";
			var regionIds = GetRegions(connection, RegionCodeEqual);

			var sql = $@"select c.CatalogName, p.ProducerName, r.RegionName, s.SupplierName,
T.Summ, CAST(T.PosOrder as SIGNED INTEGER) as PosOrder,
T.DistinctOrderId, T.DistinctAddressId
from
	(select CatalogId, ProducerId, RegionCode, SupplierId,
	Sum(Cost*Quantity) as Summ,
	Sum(Quantity) as PosOrder,
	Count(distinct OrderId) as DistinctOrderId,
	Count(distinct AddressId) as DistinctAddressId
	from producerinterface.RatingReportOrderItems
		{join}
	where RegionCode in ({regionIds})
	and SupplierId in
		(select Id
		from Customers.Suppliers
		where IsVirtual = 0 and IsFederal = 0 and RegionMask & @RegionMask)
	and WriteTime > @DateFrom
	and WriteTime < @DateTo
	{filter}
	group by CatalogId,ProducerId,RegionCode,SupplierId) as T
left join producerinterface.CatalogNames c on c.CatalogId = T.CatalogId
left join producerinterface.ProducerNames p on p.ProducerId = T.ProducerId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId
order by c.CatalogName asc, T.PosOrder desc";
			var cmd = new MySqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@DateFrom", DateFrom);
			cmd.Parameters.AddWithValue("@DateTo", DateTo);
			cmd.Parameters.AddWithValue("RegionMask", RegionCodeEqual.Select(x => (ulong)x).Aggregate((y, z) => y | z));
			return cmd;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			return new Dictionary<string, object> {
				{"RegionCodeEqual", h.GetRegionList(Id)},
				{"CatalogIdEqual", h.GetCatalogList()}
			};
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (!AllCatalog && (CatalogIdEqual == null || CatalogIdEqual.Count == 0))
        errors.Add(new ErrorMessage("CatalogIdEqual", "Не выбраны товары"));
      return errors;
		}

		public override IProcessor GetProcessor()
		{
			return new Processor<SecondarySalesReportRow>();
		}
	}
}