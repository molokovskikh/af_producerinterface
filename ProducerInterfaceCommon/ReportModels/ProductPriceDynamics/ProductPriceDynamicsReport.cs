using System;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductPriceDynamicsReport : IntervalReport
	{
		public override string Name
		{
			get { return "Динамика цен и остатков по товару за период"; }
		}

		[Display(Name = "Регион")]
		[Required(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("LongList")]
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

		public override string GetSpName()
		{
			return "ProductPriceDynamicsReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();

			if (AllCatalog)
			{
				spparams.Add("@CatalogId", $"select CatalogId from Catalogs.assortment where ProducerId = {ProducerId}");
			}
			else
			{
				spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			}
			spparams.Add("@ProducerId", ProducerId);
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));

			if (SupplierIdEqual == null)
				spparams.Add("@SupplierId", "select Id from Customers.Suppliers");
			else
				spparams.Add("@SupplierId", String.Join(",", SupplierIdEqual));

			// чтоб правильно работала хп при отсутствии ограничений на поставщиков, заведомо несуществующий Id
			if (SupplierIdNonEqual == null)
				spparams.Add("@NotSupplierId", -1);
			else
				spparams.Add("@NotSupplierId", String.Join(",", SupplierIdNonEqual));

			spparams.Add("@DateFrom", DateFrom);
			spparams.Add("@DateTo", DateTo);
			return spparams;
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
