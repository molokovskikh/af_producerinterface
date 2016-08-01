using System;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion
{
	public class SearchProducerPromotion
	{
		public SearchProducerPromotion()
		{
			Begin = DateTime.Now.AddDays(-30);
			End = DateTime.Now.AddDays(30);
			EnabledDateTime = false;
		}

		public ActualPromotionStatus? Status { get; set; }
		public long Producer { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
		public bool EnabledDateTime { get; set; }

		public object Find(Context db2)
		{
			var query = db2.Promotions.AsQueryable();
			if (!EnabledDateTime)
				query = query.Where(x => x.Begin > Begin && x.End < End);
			if (Producer > 0)
				query = query.Where(x => x.ProducerId == Producer);

			var items = query.OrderByDescending(x => x.UpdateTime).ToList();
			if (Status != null)
				items = items.Where(x => x.GetStatus() == Status.Value).ToList();
			return items;
		}
	}
}
