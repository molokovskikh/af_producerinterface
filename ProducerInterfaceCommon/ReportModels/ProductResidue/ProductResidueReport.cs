using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductResidueReport : NotIntervalReport
	{
		public override string Name
		{
			get { return "Мониторинг остатков у дистрибьюторов"; }
		}

		[Display(Name = "Регион")]
		[Required(ErrorMessage = "Не указаны регионы")]
		[UIHint("DecimalList")]
		public List<decimal> RegionCodeEqual { get; set; }

		[Display(Name = "Товар")]
		[UIHint("LongList")]
		public List<long> CatalogIdEqual { get; set; }

		[Display(Name = "Отчет готовится по")]
		[Required(ErrorMessage = "Не указан вариант подготовки отчета")]
		[UIHint("CatalogVar")]
		public CatalogVar Var { get; set; }

		public ProductResidueReport()
		{
			Var = CatalogVar.AllCatalog;
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom));
			result.Add(h.GetRegionHeader(RegionCodeEqual));

			// если выбрано По всему ассортименту
			if (Var == CatalogVar.AllAssortiment)
				result.Add("В отчет включены все товары всех производителей");
			// если выбрано По всем нашим товарам
			else if (Var == CatalogVar.AllCatalog)
				result.Add("В отчет включены все товары производителя");
			else
				result.Add(h.GetProductHeader(CatalogIdEqual));
			return result;
		}

		public override string GetSpName()
		{
			var now = DateTime.Now;
			// если на момент запуска
			if (DateFrom == new DateTime(now.Year, now.Month, now.Day))
				return "ProductResidueReportNow";
			// если на другой день
			else
				return "ProductResidueReport";
		}

		public override Dictionary<string, object> GetSpParams()
		{
			var spparams = new Dictionary<string, object>();
			if (Var == CatalogVar.AllAssortiment) {
				spparams.Add("@CatalogId", "select CatalogId from Catalogs.assortment");
			}
			else if(Var == CatalogVar.AllCatalog) {
				spparams.Add("@CatalogId", $"select CatalogId from Catalogs.assortment where ProducerId = {ProducerId}");
			}
			else {
				spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			}
			spparams.Add("@RegionCode", String.Join(",", RegionCodeEqual));
			spparams.Add("@ProducerId", ProducerId);
			spparams.Add("@DateFrom", DateFrom);
			return spparams;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();

			viewDataValues.Add("RegionCodeEqual", h.GetRegionList());
			viewDataValues.Add("CatalogIdEqual", h.GetCatalogList());

			return viewDataValues;
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
			return new Processor<ProductResidueReportRow>();
		}
	}
}