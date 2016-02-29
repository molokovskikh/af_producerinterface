using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
    public class SearchModel
    {
        public long ProducerId { get; set; }
        public bool Producer { get; set; }
        public bool DateTimeApply { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }
}
