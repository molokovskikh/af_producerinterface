using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Report
{
    public class SearchProducerReportsModel
    {
        [UIHint("Producer")]
        [Display(Name = "Производитель")]
        public long? Producer { get; set; }

        [Display(Name = "Активность")]
        public bool Enable { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int CurrentPageIndex { get; set; }

    }
}
