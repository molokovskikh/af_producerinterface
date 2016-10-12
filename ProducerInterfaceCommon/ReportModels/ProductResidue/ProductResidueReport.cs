using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ProducerInterfaceCommon.Helpers;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public class ProductResidueReport : NotIntervalReport
	{
		public override string Name => "Мониторинг остатков у дистрибьюторов";

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

		[Display(Name = "Только указанные поставщики")]
		[UIHint("LongList")]
		public List<long> SupplierIdEqual { get; set; }

		[Display(Name = "Игнорируемые поставщики")]
		[UIHint("LongList")]
		public List<long> SupplierIdNonEqual { get; set; }

		public ProductResidueReport()
		{
			Var = CatalogVar.AllCatalog;
			RegionCodeEqual = new List<decimal>();
			CatalogIdEqual = new List<long>();
			SupplierIdEqual = new List<long>();
			SupplierIdNonEqual = new List<long>();
		}

		public override void Init(ProducerInterfaceCommon.ContextModels.Account currentUser)
		{
			base.Init(currentUser);
			if (ProducerId == null)
				Var = CatalogVar.AllAssortment;
		}

		public override List<string> GetHeaders(HeaderHelper h)
		{
			var result = new List<string>();
			result.Add(h.GetDateHeader(DateFrom));
			result.Add(h.GetRegionHeader(RegionCodeEqual));

			// если выбрано По всему ассортименту
			if (Var == CatalogVar.AllAssortment)
				result.Add("В отчет включены все товары всех производителей");
			// если выбрано По всем нашим товарам
			else if (Var == CatalogVar.AllCatalog)
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
			if (Var == CatalogVar.AllAssortment) {
				spparams.Add("@CatalogId", "select CatalogId from Catalogs.assortment");
			}
			else if(Var == CatalogVar.AllCatalog) {
				spparams.Add("@CatalogId", $"select CatalogId from Catalogs.assortment where ProducerId = {ProducerId}");
			}
			else {
				spparams.Add("@CatalogId", String.Join(",", CatalogIdEqual));
			}
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
			return spparams;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			return new Dictionary<string, object> {
				{"RegionCodeEqual", h.GetRegionList(Id)},
				{"CatalogIdEqual", h.GetCatalogList()},
				{"SupplierIdEqual", h.GetSupplierList(RegionCodeEqual)},
				{"SupplierIdNonEqual", h.GetSupplierList(RegionCodeEqual)}
			};
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = base.Validate();
			if (SupplierIdEqual != null && SupplierIdNonEqual != null && SupplierIdEqual.Intersect(SupplierIdNonEqual).Any()) {
				errors.Add(new ErrorMessage("SupplierIdEqual", "Один и тот же поставщик не может одновременно входить в список выбранных и игнорируемых"));
				errors.Add(new ErrorMessage("SupplierIdNonEqual", "Один и тот же поставщик не может одновременно входить в список выбранных и игнорируемых"));
			}
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