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
			var results = _cntx.Regions()
				.Select(x => new OptionElement { Value = x.Id.ToString(), Text = x.Name })
				.ToList();
			return results;
		}

		// #48585 возвращает список регионов, разрешённый для данного типа отчётов
		public List<OptionElement> GetRegionList(int reportId)
		{
			var userRegionCodes = _cntx.AccountRegion.Where(x => x.AccountId == _userId).ToList().Select(x => x.RegionId).ToList();
			var reportRegionCodes = _cntx.ReportRegion.Where(x => x.ReportId == reportId).ToList().Select(x => x.RegionId).ToList();
			var intersect = userRegionCodes.Intersect(reportRegionCodes);

			var results = _cntx.Regions()
				.Where(x => intersect.Contains(x.Id))
				.ToList()
				.OrderBy(x => x.Name)
				.Select(x => new OptionElement { Value = x.Id.ToString(), Text = x.Name })
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

			var LisrRegions = _cntx.Regions().ToList();

			var results = LisrRegions
				.Where(x =>(x.Id & RegionMask) > 0)
				.OrderBy(x => x.Id)
				.Select(x => (long)x.Id).ToList();
			return results;
		}

		public List<OptionElement> GetPromotionRegionNames(ulong RegionMask)
		{
			var ids = GetPromotionRegions(RegionMask);
			return _cntx.Regions().Where(x => ids.Contains((long)x.Id)).ToList().Select(x => new OptionElement { Text = x.Name, Value = x.Id.ToString() }).ToList();
		}

		public List<OptionElement> RegisterListProducer()
		{
			var producerListLong = _cntx.AccountCompany.Where(x => x.ProducerId.HasValue).Select(x => x.ProducerId).ToList().Distinct();

			var listProducer = _cntx.producernames
					.Where(x => producerListLong.Contains(x.ProducerId))
					.Select(v => new OptionElement { Text = v.ProducerName, Value = v.ProducerId.ToString() }).ToList();
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
