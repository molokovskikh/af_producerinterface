using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProducerInterfaceCommon.Heap
{
	public class NamesHelper
	{

		private ProducerInterfaceCommon.ContextModels.producerinterface_Entities _cntx;

		private long _userId;

		public NamesHelper(ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx, long userId)
		{
			_cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
			_userId = userId;
		}

		// TODO почта откуда-то берётся
		// для UI
		public List<OptionElement> GetMailList()
		{
			var results = new List<OptionElement>();

			//results.Add(new OptionElement() { Text = "g.maksimenko@analit.net", Value = "g.maksimenko@analit.net" });
			//results.Add(new OptionElement() { Text = "y.borisov@analit.net", Value = "y.borisov@analit.net" });
			//results.Add(new OptionElement() { Text = "michail@analit.net", Value = "michail@analit.net" });
			//results.Add(new OptionElement() { Text = "r.kvasov@analit.net", Value = "r.kvasov@analit.net" });

			// TODO: до переноса в ProducerInterface пользователя может и не быть, далее он обязан быть
			var u = _cntx.Account.SingleOrDefault(x => x.Id == _userId);
			if (u != null)
				results.Add(new OptionElement() { Text = u.Login, Value = u.Login });

			return results;
		}

		// для UI ???
		public string GetMailOkReportSubject()
		{
			// TODO: до переноса в ProducerInterface пользователя может и не быть, далее он обязан быть
			var result = "Отчет пользователя *** производителя *** (будет указано после переноса в ProducerInterface)";
			var u = _cntx.Account.SingleOrDefault(x => x.Id == _userId);
			if (u != null)
			{
				var X = _cntx.producernames.Where(xxx => xxx.ProducerId == u.AccountCompany.ProducerId).FirstOrDefault();
				string companyName = "";
				if (X != null && X.ProducerId != 0) { companyName = X.ProducerName; }
				else { companyName = u.AccountCompany.Name; }


				result = $"Отчет пользователя {u.Name} {u.Login} производителя {companyName}";
			}
			return result;
		}

		// для UI
		public List<OptionElement> GetRegionList()
		{
			var results = _cntx.regionnames
				.OrderBy(x => x.RegionName)
				.Select(x => new OptionElement { Value = x.RegionCode.ToString(), Text = x.RegionName })
				.ToList();
			return results;
		}

		// для UI
		public List<OptionElement> GetCatalogList()
		{
			var producerId = _cntx.Account.Single(x => x.Id == _userId).AccountCompany.ProducerId;
			var results = _cntx.assortment
				.Where(x => x.ProducerId == producerId)
				.OrderBy(x => x.CatalogName)
				.Select(x => new OptionElement { Value = x.CatalogId.ToString(), Text = x.CatalogName })
				.ToList();
			return results;
		}

		public List<OptionElement> GetCatalogListPromotion()
		{
			var producerId = _cntx.Account.Single(x => x.Id == _userId).AccountCompany.ProducerId;
			var NewListDrug = _cntx.promotionToDrug.Where(xxx => _cntx.promotions.Where(xx => xx.ProducerId == producerId)
					.Select(xx => xx.Id).ToList().Contains(xxx.PromotionId)).Select(x => x.DrugId).ToList();
			var results = _cntx.assortment.Where(xxx => NewListDrug.Contains(xxx.CatalogId))
					.GroupBy(x => x.CatalogId).Select(x => x.FirstOrDefault())
					.Select(xxx => new OptionElement { Text = xxx.CatalogName, Value = xxx.CatalogId.ToString() })
					.ToList();
			return results;
		}

		public List<OptionElement> GetDrugList(List<long> SelectedDrugs)
		{
			if (SelectedDrugs == null || SelectedDrugs.Count() == 0)
				return new List<OptionElement>();

			var ListDrugsNoConvert = _cntx.drugfamilynames.Where(x => SelectedDrugs.Contains(x.FamilyId)).ToList();
			var ListSelectGrugs = ListDrugsNoConvert.Select(xxx => new OptionElement { Text = xxx.FamilyName, Value = xxx.FamilyId.ToString() }).ToList();
			return ListSelectGrugs;
		}

		public List<OptionElement> GetSearchCatalogFamalyName(string NameDrug)
		{
			var result = _cntx.drugfamilynames.Where(xxx => xxx.FamilyName.Contains(NameDrug)).Take(10).ToList().Select(xxx => new OptionElement { Value = xxx.FamilyId.ToString(), Text = xxx.FamilyName }).ToList();
			return result;
		}

		// для UI
		public List<OptionElement> GetSupplierList(List<decimal> regionList)
		{
			// если регионы не указаны - возвращаем пустой лист
			if (regionList == null)
				return new List<OptionElement>();

			var suppliersInRegions = _cntx.supplierregions.Where(x => regionList.Contains(x.RegionCode.Value)).Select(x => x.SupplierId).ToList();
			var results = _cntx.suppliernames
				.Where(x => suppliersInRegions.Contains(x.SupplierId))
				.OrderBy(x => x.SupplierName)
				.Select(x => new OptionElement { Value = x.SupplierId.ToString(), Text = x.SupplierName })
				.ToList();

			//var regionMask = regionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);
			//var suppliers = _cntx.suppliernames.ToList();
			//var results = suppliers
			//	.Where(x => ((ulong)x.HomeRegion.Value & regionMask) > 0)
			//	.OrderBy(x => x.SupplierName)
			//	.Select(x => new OptionElement { Value = x.SupplierId.ToString(), Text = x.SupplierName }).ToList();
			return results;
		}

		public List<long> GetPromotionRegions(ulong? RegionMask)
		{
			if (RegionMask == null)
			{
				return new List<long>();
			}

            if (RegionMask == 0)
            {
                return new List<long>(){ 0 };
            }

			var LisrRegions = _cntx.regionnames.OrderBy(x => x.RegionName).ToList();

			var results = LisrRegions
			.Where(x =>
			((ulong)x.RegionCode & RegionMask) > 0)
			.OrderBy(x => x.RegionCode)
			.Select(x => (long)x.RegionCode).ToList();
			return results;
		}

        public List<OptionElement> RegisterListProducer()
        {
            var producerListLong =_cntx.AccountCompany.Where(zzz => zzz.ProducerId != null)
                .GroupBy(x => x.ProducerId).Select(x => x.FirstOrDefault()).Select(xxx => xxx.ProducerId).ToList();

          
            var listProducer = _cntx.producernames
                .Where(xxx => producerListLong.Contains(xxx.ProducerId))
                .ToList().Select(v => new OptionElement { Text = v.ProducerName, Value = v.ProducerId.ToString() }).ToList();
            return listProducer;
        }

        public List<OptionElement> GetProducerUserList(long ProducerId)
        {
            return _cntx.Account.Where(xxx => xxx.AccountCompany.ProducerId == ProducerId)
                .ToList().Select(xxx => new OptionElement { Text = xxx.Name, Value = xxx.Id.ToString() })
                .ToList();
        }

        public List<OptionElement> GetProducerList()
		{
			var X = _cntx.producernames.ToList().Select(xxx => new OptionElement { Text = xxx.ProducerName, Value = xxx.ProducerId.ToString() }).ToList();
			var Y = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
			Y.AddRange(X);

			return Y;
		}
	}
}
