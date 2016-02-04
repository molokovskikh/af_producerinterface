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
			var u =_cntx.Account.SingleOrDefault(x => x.Id == _userId);
			if (u != null)
				results.Add(new OptionElement() { Text = u.Login, Value = u.Login });

			return results;
		}
     
        // для UI ???
        public string GetMailOkReportSubject()
		{
			// TODO: до переноса в ProducerInterface пользователя может и не быть, далее он обязан быть
			var result = "Отчет пользователя *** производителя *** (будет указано после переноса в ProducerInterface)";
			var u =_cntx.Account.SingleOrDefault(x => x.Id == _userId);
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

			var regionMask = regionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);
			var suppliers = _cntx.suppliernames.ToList();
			var results = suppliers
				.Where(x => ((ulong)x.HomeRegion.Value & regionMask) > 0)
				.OrderBy(x => x.SupplierName)
				.Select(x => new OptionElement { Value = x.SupplierId.ToString(), Text = x.SupplierName }).ToList();
			return results;
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
