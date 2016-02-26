using System;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Promotion
{
    public class SearchProducerPromotion
    {
        public byte Status { get; set; }
        public long Producer { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public bool EnabledDateTime { get; set; }
    }
}
