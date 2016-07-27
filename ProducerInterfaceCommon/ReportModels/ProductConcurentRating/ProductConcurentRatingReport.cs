using System;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using System.ComponentModel.DataAnnotations;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductConcurentRatingReport : IntervalReport
	{
		[Display(Name = "Регион")]
		[CollectionRequired(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Выберите собственные товары")]
		[CollectionRequired(ErrorMessage = "Не выбраны товары")]
		[UIHint("Products")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "Выберите товары, конкурирующие с вашими (не более 50)")]
		[CollectionRequired(ErrorMessage = "Не выбраны товары")]
		[UIHint("LongList")]
		public List<long> CatalogIdEqual2 { get; set; }

		public override string Name => "Рейтинг товаров в конкурентной группе";

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom, DateTo));
			result.Add(h.GetRegionHeader(RegionCodeEqual));
			var c = new List<long>();
			c.AddRange(CatalogIdEqual);
			c.AddRange(CatalogIdEqual2);
			result.Add(h.GetProductHeader(c));
			return result;
		}

		public override string GetSpName()
		{
			return "ProductConcurentRatingReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			var c = new List<long>();
			c.AddRange(CatalogIdEqual);
			c.AddRange(CatalogIdEqual2);
			spparams.Add("@CatalogId", String.Join(",", c));
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
			spparams.Add("@DateFrom", DateFrom);
			spparams.Add("@DateTo", DateTo);
			return spparams;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList(Id));
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());
			viewDataValues.Add("CatalogIdEqual2", h.GetCatalogList(CatalogIdEqual2));

			return viewDataValues;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (CatalogIdEqual2 != null && CatalogIdEqual2.Count > 50)
				errors.Add(new ErrorMessage("CatalogIdEqual2", "Можно выбрать не более 50 товаров конкурентов"));
			return errors;
		}

		public override IProcessor GetProcessor()
		{
			return new Processor<ProductConcurentRatingReportRow>();
		}
	}
}
