using System;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;
using Common.Tools;
using MySql.Data.MySqlClient;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductPriceDynamicsReport : IntervalReport
	{
		public override string Name => "Динамика цен и остатков по товару за период";

		[Display(Name = "Регион")]
		[Required(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("Products")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "По всем нашим товарам")]
		[UIHint("Bool")]
		public bool AllCatalog { get; set; }

		[Display(Name = "Отображать")]
		[Required(ErrorMessage = "Не указан вариант подготовки отчета")]
		[UIHint("CostOrQuantity")]
		public CostOrQuantity VarCostOrQuantity { get; set; }

		[Display(Name = "Только указанные поставщики")]
		[UIHint("LongList")]
		public List<long> SupplierIdEqual { get; set; }

		[Display(Name = "Игнорируемые поставщики")]
		[UIHint("LongList")]
		public List<long> SupplierIdNonEqual { get; set; }

		public ProductPriceDynamicsReport()
		{
			AllCatalog = true;
			VarCostOrQuantity = CostOrQuantity.WithCostAndQuantity;
			CatalogIdEqual = new List<long>();
			RegionCodeEqual = new List<decimal>();
			SupplierIdEqual = new List<long>();
			SupplierIdNonEqual = new List<long>();
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom, DateTo));
			result.Add(h.GetRegionHeader(RegionCodeEqual));

			// если выбрано По всем нашим товарам
			if (AllCatalog)
				result.Add("В отчет включены все товары производителя");
			else
				result.Add(h.GetProductHeader(CatalogIdEqual));

			if (SupplierIdEqual != null)
				result.Add(h.GetSupplierHeader(SupplierIdEqual));

			if (SupplierIdNonEqual != null)
				result.Add(h.GetNotSupplierHeader(SupplierIdNonEqual));

			return result;
		}

		public override MySqlCommand GetCmd(MySqlConnection connection)
		{
			var filter = "";
			var join = "";
			if (AllCatalog)
			{
				if (ProducerId != null)
					join = $"join Catalogs.Assortment a on a.CatalogId = p.CatalogId and a.ProducerId = {ProducerId}";
			} else {
				filter += $" and p.CatalogId in {CatalogIdEqual.Implode()}";
			}
			if (ProducerId != null)
				filter += $" and ProducerId = {ProducerId}";
			var regionIds = GetRegions(connection, RegionCodeEqual);

			if (SupplierIdEqual.Count > 0)
				filter += $" and SupplierId in ({SupplierIdEqual.Implode()})";

			if (SupplierIdNonEqual.Count > 0)
				filter += $" and SupplierId not in ({SupplierIdNonEqual.Implode()})";

			var sql = $@"select cn.CatalogName, p.ProducerName, r.RegionName, s.SupplierName, TT.Date, TT.Cost, CAST(TT.Quantity as SIGNED INTEGER) as Quantity from
	(select p.CatalogId, T.ProducerId, T.RegionId, T.SupplierId, T.Date, AVG(T.Cost) as Cost, SUM(Quantity) as Quantity from
		(select ProductId, ProducerId, RegionId, SupplierId, Date, Cost, Quantity
		from reports.AverageCosts a
			join Catalogs.Products p on p.Id = a.ProductId
			{join}
		where Date >= @DateFrom
		and Date < @DateTo
		and RegionId in ({regionIds})
		{filter}) as T
	inner join catalogs.Products p on p.Id = T.ProductId
	group by p.CatalogId, T.ProducerId, T.RegionId, T.SupplierId, T.Date) as TT
left outer join producerinterface.regionnames r on r.RegionCode = TT.RegionId
left outer join producerinterface.catalognames cn on cn.CatalogId = TT.CatalogId
left outer join producerinterface.producernames p on p.ProducerId = TT.ProducerId
left outer join producerinterface.suppliernames s on s.SupplierId = TT.SupplierId
order by cn.CatalogName, p.ProducerName, r.RegionName, s.SupplierName, TT.Date";
			var cmd = new MySqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@DateFrom", DateFrom);
			cmd.Parameters.AddWithValue("@DateTo", DateTo);
			return cmd;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList(Id));
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());
			viewDataValues.Add("SupplierIdEqual", h.GetSupplierList(RegionCodeEqual));
			viewDataValues.Add("SupplierIdNonEqual", h.GetSupplierList(RegionCodeEqual));

			return viewDataValues;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (SupplierIdEqual != null && SupplierIdNonEqual != null && SupplierIdEqual.Intersect(SupplierIdNonEqual).Any())
			{
				errors.Add(new ErrorMessage("SupplierIdEqual", "Один и тот же поставщик не может одновременно входить в список выбранных и игнорируемых"));
				errors.Add(new ErrorMessage("SupplierIdNonEqual", "Один и тот же поставщик не может одновременно входить в список выбранных и игнорируемых"));
			}
			if (!AllCatalog && (CatalogIdEqual == null || CatalogIdEqual.Count == 0))
				errors.Add(new ErrorMessage("CatalogIdEqual", "Не выбраны товары"));
			return errors;
		}

		public override IProcessor GetProcessor()
		{
			return new Processor<ProductPriceDynamicsReportRow>();
		}
	}
}
