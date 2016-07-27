using ProducerInterfaceCommon.Heap;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Tools;
using MySql.Data.MySqlClient;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class SupplierRatingReport : IntervalReport
	{
		public override string Name => "Рейтинг поставщиков";

		[Display(Name = "Регион")]
		[Required(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("Products")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "По всем нашим товарам")]
		[UIHint("ProductsFlag")]
		public bool AllCatalog { get; set; }

		public SupplierRatingReport()
		{
			AllCatalog = true;
			RegionCodeEqual = new List<decimal>();
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			return new List<string> {
				h.GetDateHeader(DateFrom, DateTo),
				h.GetRegionHeader(RegionCodeEqual),
				GetCatalogHeader(h, AllCatalog, CatalogIdEqual)
			};
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

			var sql = $@"select s.SupplierName, r.RegionName, T.Summ
from
	(select SupplierId, RegionCode,
	Sum(Cost*Quantity) as Summ
	from producerinterface.RatingReportOrderItems
		{join}
	where IsLocal = 0
	and RegionCode in ({regionIds})
	{filter}
	and WriteTime > @DateFrom
	and WriteTime < @DateTo
	group by SupplierId,RegionCode
	order by Summ desc) as T
left join producerinterface.SupplierNames s on s.SupplierId = T.SupplierId
left join producerinterface.RegionNames r on r.RegionCode = T.RegionCode";
			var cmd = new MySqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@DateFrom", DateFrom);
			cmd.Parameters.AddWithValue("@DateTo", DateTo);
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
			return new Processor<SupplierRatingReportRow>();
		}
	}
}