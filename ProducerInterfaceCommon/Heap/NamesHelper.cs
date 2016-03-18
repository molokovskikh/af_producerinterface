using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.Heap
{
	public class NamesHelper
	{

		private ProducerInterfaceCommon.ContextModels.producerinterface_Entities _cntx;		
		private long _userId;
		private HttpContextBase _httpContext;

		public NamesHelper(ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx, long userId, HttpContextBase httpContext = null)
		{
			_cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
			_userId = userId;
			_httpContext = httpContext;
		}

		// TODO почта откуда-то берётся
		// для UI
		public List<OptionElement> GetMailList()
		{
			var results = new List<OptionElement>();

			// TODO: до переноса в ProducerInterface пользователя может и не быть, далее он обязан быть
			var u = _cntx.Account.SingleOrDefault(x => x.Id == _userId);
			if (u != null)
				results.Add(new OptionElement() { Text = u.Login, Value = u.Login });

			return results;
		}

		public string GetReportName(string jobName)
		{
			var report = _cntx.jobextend.SingleOrDefault(x => x.JobName == jobName);
			if (report == null)
				return "";
			//var producerName = _cntx.producernames.Single(x => x.ProducerId == report.ProducerId).ProducerName;
			return report.CustomName;
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

		private List<ulong> GetRegionListId(decimal RegionMask, List<regionnames> LisrRegions)
		{

			var results = LisrRegions
					.Where(x => ((ulong)x.RegionCode & (ulong)RegionMask) > 0)
					.OrderBy(x => x.RegionCode)
					.Select(x => (ulong)x.RegionCode).ToList();

			return results;
		}

		public List<OptionElement> GetRegionList(decimal RegionMask)
		{

			if (RegionMask == 0)
			{
				return _cntx.regionnames.Where(x => x.RegionCode == 0).ToList().Select(x => new OptionElement { Text = x.RegionName, Value = x.RegionCode.ToString() }).ToList();
			}

			var LisrRegions = _cntx.regionnames.OrderBy(x => x.RegionName).ToList();
			var IdGetRegions = GetRegionListId(RegionMask, LisrRegions);
			return LisrRegions.Where(x => IdGetRegions.Contains((ulong)x.RegionCode)).ToList()
											.Select(x => new OptionElement { Text = x.RegionName, Value = x.RegionCode.ToString() }).ToList();
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

		public List<OptionElement> GetCatalogList(List<long> list)
		{
			if (list == null || list.Count == 0)
				return new List<OptionElement>();
			var results = _cntx.catalognames
				.Where(x => list.Contains(x.CatalogId))
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

        public List<OptionElement> GetSupplierList(List<ulong> regionList)
        {
            if (regionList == null || regionList.Count() == 0)
            {
                return new List<OptionElement>();
            }

            List<supplierregions> suppliers = new List<supplierregions>();

            if (_httpContext != null)
            {
                suppliers = GetSuppliersInRegions().ToList();
            }
            else
            {
                suppliers = _cntx.supplierregions.ToList();
            }
            List<long> supplierIds = null;

            // если список регионов содержит 0 (все регионы) - возвращаем всех поставщиков
            if (regionList.Contains(0))
            {
                supplierIds = suppliers.Select(x => x.SupplierId).ToList();
            }
            else
            {
                var regionMask = regionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);
                supplierIds = suppliers.Where(x => ((ulong)x.RegionMask & regionMask) > 0).Select(x => x.SupplierId).ToList();
            }

            var results = _cntx.suppliernames
                .Where(x => supplierIds.Contains(x.SupplierId))
                .OrderBy(x => x.SupplierName)
                .Select(x => new OptionElement { Value = x.SupplierId.ToString(), Text = x.SupplierName })
                .ToList();
            return results;
        }

        // для UI
        public List<OptionElement> GetSupplierList(List<decimal> regionList)
		{
            // если регионы не заданы - пустой лист
            if (regionList == null || regionList.Count() == 0)
            {
                return new List<OptionElement>();
            }

            List<supplierregions> suppliers = new List<supplierregions>();

            if (_httpContext != null)
            {
                suppliers = GetSuppliersInRegions().ToList();
            }
            else
            {
                suppliers = _cntx.supplierregions.ToList();
            }
			List<long> supplierIds = null;

			// если список регионов содержит 0 (все регионы) - возвращаем всех поставщиков
			if (regionList.Contains(0))
			{
				supplierIds = suppliers.Select(x => x.SupplierId).ToList();
			}
			else
			{
				var regionMask = regionList.Select(x => (ulong)x).Aggregate((y, z) => y | z);
				supplierIds = suppliers.Where(x => ((ulong)x.RegionMask & regionMask) > 0).Select(x => x.SupplierId).ToList();
			}

			var results = _cntx.suppliernames
				.Where(x => supplierIds.Contains(x.SupplierId))
				.OrderBy(x => x.SupplierName)
				.Select(x => new OptionElement { Value = x.SupplierId.ToString(), Text = x.SupplierName })
				.ToList();
			return results;
		}

		private HashSet<supplierregions> GetSuppliersInRegions()
		{
			var key = $"SuppliersInReg";

			var DateList = _httpContext.Cache.Get(key) as HashSet<supplierregions>;

			if (DateList != null)
			{
				return DateList;
			}

			DateList = new HashSet<supplierregions>();

			var Data = _cntx.supplierregions.Distinct().ToList();

			foreach (var DataItem in Data)
			{
				DateList.Add(DataItem);
			}

            _httpContext.Cache.Insert(key, DateList, null, DateTime.UtcNow.AddSeconds(300), Cache.NoSlidingExpiration);
			return DateList;
		}

		public List<long> GetPromotionRegions(ulong? RegionMask)
		{
			if (RegionMask == null)
			{
				return new List<long>();
			}

			if (RegionMask == 0)
			{
				return new List<long>() { 0 };
			}

			var LisrRegions = _cntx.regionnames.OrderBy(x => x.RegionName).ToList();

			var results = LisrRegions
			.Where(x =>
			((ulong)x.RegionCode & RegionMask) > 0)
			.OrderBy(x => x.RegionCode)
			.Select(x => (long)x.RegionCode).ToList();
			return results;
		}

		public List<OptionElement> GetPromotionRegionNames(ulong RegionMask)
		{
			var RegionLongList = GetPromotionRegions((ulong)RegionMask);
			List<decimal> RLS = RegionLongList.Select(x => (decimal)x).ToList();
			return _cntx.regionnames.Where(x => RLS.Contains(x.RegionCode)).ToList().Select(x => new OptionElement { Text = x.RegionName, Value = x.RegionCode.ToString() }).ToList();
		}

		public List<OptionElement> RegisterListProducer()
		{
			var producerListLong = _cntx.AccountCompany.Where(zzz => zzz.ProducerId != null)
					.GroupBy(x => x.ProducerId).Select(x => x.FirstOrDefault()).Select(xxx => xxx.ProducerId).ToList();


			var listProducer = _cntx.producernames
					.Where(xxx => producerListLong.Contains(xxx.ProducerId))
					.ToList().Select(v => new OptionElement { Text = v.ProducerName, Value = v.ProducerId.ToString() }).ToList();
			return listProducer;
		}

		public List<OptionElement> GetDrugInPromotion(long PromotionId)
		{
			List<long> DrudIdList = _cntx.promotionToDrug.Where(x => x.PromotionId == PromotionId).Select(x => x.DrugId).ToList();
			return _cntx.assortment.ToList().Where(x => DrudIdList.Contains(x.CatalogId)).ToList().Select(x => new OptionElement { Text = x.CatalogName, Value = x.CatalogId.ToString() }).ToList();
		}

		public List<OptionElement> GetProducerUserList(long ProducerId)
		{
			return _cntx.Account.Where(xxx => xxx.CompanyId != null).Where(xxx => xxx.AccountCompany.ProducerId == ProducerId)
					.ToList().Select(xxx => new OptionElement { Text = xxx.Login + " " + xxx.Name, Value = xxx.Id.ToString() })
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
