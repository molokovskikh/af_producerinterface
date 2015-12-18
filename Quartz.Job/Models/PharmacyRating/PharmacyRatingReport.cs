using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
{
	[Serializable]
	public class PharmacyRatingReport : IntervalReport
	{
		public override string Name
		{
			get { return "Рейтинг аптек"; }
		}

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
			return result;
		}

		public override Report Process(Report param, JobKey key, bool runNow)
		{
			var castparam = base.Process(param, key, runNow);
			var processor = new Processor<PharmacyRatingReportRow>();
			processor.Process(castparam, key, runNow);
			return castparam;
		}

		public override string GetSpName()
		{
			return "PharmacyRatingReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
			spparams.Add("@DateFrom", DateFrom);
			spparams.Add("@DateTo", DateTo);
			return spparams;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList());
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());
			viewDataValues.Add("MailTo", h.GetMailList());

			return viewDataValues;
		}
	}
}