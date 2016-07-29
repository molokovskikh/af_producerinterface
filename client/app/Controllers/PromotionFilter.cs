using System.Collections.Generic;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.ViewModel.Interface.Promotion;

namespace ProducerInterface.Controllers
{
	public class PromotionFilter
	{
		public PromotionFilter()
		{
			Items = new List<PromotionUi>();
		}

		public ActualPromotionStatus? Status { get; set; }
		public List<PromotionUi> Items { get; set; }

		public void Find(producerinterface_Entities db, Context db2, long producerId)
		{
			var promoList = db2.Promotions.Where(x => x.ProducerId == producerId).OrderByDescending(x => x.Begin)
				.ThenByDescending(x => x.Id).ToList();
			var suppliers = db.suppliernames.ToDictionary(x => x.SupplierId, x => x.SupplierName);
			var assortment = db.assortment.Where(x => x.ProducerId == producerId).ToDictionary(x => x.CatalogId, x => x.CatalogName);
			foreach (var item in promoList) {
				unchecked {
					if (item.RegionMask == 0)
						item.RegionMask = (long)ulong.MaxValue;
				}
				var supplierIds = item.PromotionsToSupplier.Select(x => x.SupplierId).ToList();
				var drugsIds = item.PromotionToDrug.Select(x => x.DrugId).ToList();
				var status = item.GetStatus();
				if (Status != null && Status != status)
					continue;
				var itemUi = new PromotionUi() {
					Id = item.Id,
					Name = item.Name,
					Annotation = item.Annotation,
					Begin = item.Begin.ToString("dd.MM.yyyy"),
					End = item.End.ToString("dd.MM.yyyy"),
					PromotionFileId = item.MediaFile?.Id,
					PromotionFileName = item.MediaFile?.ImageName,
					AllSuppliers = item.AllSuppliers,
					ActualStatus = status,
					DrugList = assortment.Where(x => drugsIds.Contains(x.Key)).Select(x => x.Value).ToList(),
					RegionList = db.Regions((ulong)item.RegionMask).Select(x => x.Name).ToList(),
					SuppierRegions = suppliers.Where(x => supplierIds.Contains(x.Key)).Select(x => x.Value).ToList(),
					RowStyle = item.RowStyle
				};
				Items.Add(itemUi);
			}
		}
	}
}