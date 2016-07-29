using System;
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
	}
}
