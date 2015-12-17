using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
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
		[Required(ErrorMessage = "Не выбраны товары")]
		[UIHint("LongList")]
		public List<long> CatalogIdEqual { get; set; }

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom, DateTo));
			result.Add(h.GetRegionHeader(RegionCodeEqual));
			result.Add(h.GetProductHeader(CatalogIdEqual));
			result.Add(h.GetNotSupplierHeader(SupplierIdNonEqual));
			return result;
		}

		public override Report Process(Report param, JobKey key)
		{
			var castparam = base.Process(param, key);
			var processor = new Processor<PharmacyRatingReportRow>();
			processor.Process(castparam, key);
			return castparam;
		}

		public override string GetSpName()
		{
			return "ProductRatingReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
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
			viewDataValues.Add("MailTo", h.GetMailList());

			return viewDataValues;
		}
	}
}