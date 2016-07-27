using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Tools;
using MySql.Data.MySqlClient;
using Dapper;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductRatingReport : IntervalReport
	{
		public override string Name
		{
			get { return "Рейтинг товаров"; }
		}

		[Display(Name = "Все поставщики, кроме")]
		[UIHint("LongList")]
		public List<long> SupplierIdNonEqual { get; set; }

		[Display(Name = "Регион")]
		[CollectionRequired(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("Products")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "Отчет готовится по")]
		[Required(ErrorMessage = "Не указан вариант подготовки отчета")]
		[UIHint("CatalogVar")]
		public CatalogVar Var { get; set; }

		public ProductRatingReport()
		{
			Var = CatalogVar.AllCatalog;
			SupplierIdNonEqual = new List<long>();
			RegionCodeEqual = new List<decimal>();
			CatalogIdEqual = new List<long>();
		}

		public override void Init(Account currentUser)
		{
			base.Init(currentUser);
			if (ProducerId == null)
				Var = CatalogVar.AllAssortment;
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom, DateTo));
			result.Add(h.GetRegionHeader(RegionCodeEqual));

			// если выбрано По всему ассортименту
			if (Var == CatalogVar.AllAssortment)
				result.Add("В отчет включены все товары всех производителей");
			// если выбрано По всем нашим товарам
			else if (Var == CatalogVar.AllCatalog)
				result.Add("В отчет включены все товары производителя");
			else
				result.Add(h.GetProductHeader(CatalogIdEqual));

			if (SupplierIdNonEqual != null)
				result.Add(h.GetNotSupplierHeader(SupplierIdNonEqual));
			return result;
		}

		public override MySqlCommand GetCmd(MySqlConnection connection)
		{
			var regionIds = GetRegions(connection, RegionCodeEqual);

			var join = "";
			var filter = "";
			if (Var == CatalogVar.AllAssortment) {
				join = "join Catalogs.Assortment a on a.CatalogId = ri.CatalogId";
			} else if(Var == CatalogVar.AllCatalog) {
				join = $"join Catalogs.Assortment a on a.CatalogId = ri.CatalogId and a.ProducerId = {ProducerId}";
			} else {
				filter = $"and ri.CatalogId in ({CatalogIdEqual.Implode()})";
			}
			if (SupplierIdNonEqual.Count > 0)
				filter += $" and ri.FirmCode not in ({SupplierIdNonEqual.Implode()})";

			var sql = $@"select c.CatalogName, ri.ProducerId, p.ProducerName, r.RegionName,
Sum(ri.Cost * ri.Quantity) as Summ,
CAST(Sum(ri.Quantity) as SIGNED INTEGER) as PosOrder,
Min(ri.Cost) as MinCost,
Avg(ri.Cost) as AvgCost,
Max(ri.Cost) as MaxCost,
Count(distinct ri.OrderId) as DistinctOrderId,
Count(distinct ri.AddressId) as DistinctAddressId
from producerinterface.RatingReportOrderItems ri
	left join producerinterface.CatalogNames c on c.CatalogId = ri.CatalogId
	left join producerinterface.ProducerNames p on p.ProducerId = ri.ProducerId
	left join producerinterface.RegionNames r on r.RegionCode = ri.RegionCode
	{join}
where ri.IsLocal = 0
	{filter}
	and ri.RegionCode in ({regionIds})
	and ri.WriteTime > @DateFrom
	and ri.WriteTime < @DateTo
group by ri.CatalogId, ri.ProducerId, ri.RegionCode
order by Summ desc";
			var cmd = new MySqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("DateFrom", DateFrom);
			cmd.Parameters.AddWithValue("DateTo", DateTo);
			return cmd;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			return new Dictionary<string, object> {
				{"RegionCodeEqual", h.GetRegionList(Id)},
				{"CatalogIdEqual", h.GetCatalogList()},
				{"SupplierIdNonEqual", h.GetSupplierList(RegionCodeEqual)}
			};
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (Var == CatalogVar.SelectedProducts && (CatalogIdEqual == null || CatalogIdEqual.Count == 0))
				errors.Add(new ErrorMessage("CatalogIdEqual", "Не выбраны товары"));
			if (Var == 0)
				errors.Add(new ErrorMessage("Var", "Не указан вариант подготовки отчета"));
			return errors;
		}

		public override IProcessor GetProcessor()
		{
			return new Processor<ProductRatingReportRow>();
		}
	}
}