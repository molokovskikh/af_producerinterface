using ProducerInterfaceCommon.Heap;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
		[Required(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("LongList")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "По всем нашим товарам")]
		[UIHint("Bool")]
		public bool AllCatalog { get; set; }

		[Display(Name = "По всему ассортименту")]
		[UIHint("Bool")]
		public bool AllAssortiment { get; set; }

		public ProductRatingReport()
		{
			AllCatalog = true;
			AllAssortiment = false;
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom, DateTo));
			result.Add(h.GetRegionHeader(RegionCodeEqual));

			// если выбрано По всему ассортименту
			if (AllAssortiment)
				result.Add("В отчет включены все товары всех производителей");
			// если выбрано По всем нашим товарам
			else if (AllCatalog)
				result.Add("В отчет включены все товары производителя");
			else
				result.Add(h.GetProductHeader(CatalogIdEqual));

			if (SupplierIdNonEqual != null)
				result.Add(h.GetNotSupplierHeader(SupplierIdNonEqual));
			return result;
		}

		public override string GetSpName()
		{
			return "ProductRatingReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			if (AllAssortiment) {
				spparams.Add("@CatalogId", "select CatalogId from Catalogs.assortment");
			}
			else if(AllCatalog) {
				spparams.Add("@CatalogId", $"select CatalogId from Catalogs.assortment where ProducerId = {ProducerId}");
			}
			else {
				spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			}
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
			// чтоб правильно работала хп при отсутствии ограничений на поставщиков, заведомо несуществующий Id
			if (SupplierIdNonEqual == null)
				spparams.Add("@SupplierId", -1);
			else
				spparams.Add("@SupplierId", String.Join(",", SupplierIdNonEqual));
			spparams.Add("@DateFrom", DateFrom);
			spparams.Add("@DateTo", DateTo);
			return spparams;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList());
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());
			viewDataValues.Add("SupplierIdNonEqual", h.GetSupplierList(RegionCodeEqual));

			return viewDataValues;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (!AllAssortiment && !AllCatalog && (CatalogIdEqual == null || CatalogIdEqual.Count == 0))
        errors.Add(new ErrorMessage("CatalogIdEqual", "Не выбраны товары"));
      return errors;
		}

		public override IProcessor GetProcessor()
		{
			return new Processor<ProductRatingReportRow>();
		}
	}
}