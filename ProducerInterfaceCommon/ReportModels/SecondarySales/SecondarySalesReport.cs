using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class SecondarySalesReport : IntervalReport
	{
		public override string Name
		{
			get { return "Продажи вторичных дистрибьюторов"; }
		}

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

		public SecondarySalesReport()
		{
			AllCatalog = true;
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

			return result;
		}

		public override string GetSpName()
		{
			return "SecondarySalesReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			if(AllCatalog) {
				spparams.Add("@CatalogId", $"select CatalogId from Catalogs.assortment where ProducerId = {ProducerId}");
			}
			else {
				spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			}

			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
			spparams.Add("@RegionMask", RegionCodeEqual.Select(x => (ulong)x).Aggregate((y, z) => y | z));
			spparams.Add("@ProducerId", ProducerId);
			spparams.Add("@DateFrom", DateFrom);
			spparams.Add("@DateTo", DateTo);
			return spparams;

    }

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList(Id));
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());

			return viewDataValues;
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