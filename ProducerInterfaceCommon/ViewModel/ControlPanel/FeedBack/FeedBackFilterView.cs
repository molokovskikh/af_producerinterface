using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
    public class FeedBackList
    {
        public List<feedbackui> FeedList { get; set; }
        public List<Paginator> PaginatorLinks { get; set; }

        public int PageIndex { get; set; }

        public string SortTime { get; set; }
        public string SortProducerName { get; set; }
        public string SortAccountName { get; set; }
        public string SortType { get; set; }
        public string SortStatus { get; set; }
    }

    public class FeedBackItemSelect : AccountFeedBack
    {
        public List<SelectListItem> StatusList { get; set; }
    }

    // данная модель будет возвращатся при поиске результатов с клиента
    public class FeedBackFilter
    {    
        public string DateBegin { get; set; }    
        public string DateEnd { get; set; }
        public int PageIndex { get; set; }
        public long ProducerId { get; set; }
        public long AccountId { get; set; }
        public int ItemsPerPage { get; set; }

        public List<OptionElement> ProducerList { get; set; }
        public List<OptionElement> AccountList { get; set; }
        public List<OptionElement> ItemsPerPageList { get; set;}       
    }
       
    public class Paginator
    {
        public int Counter { get; set; }
        public int ViewCounter { get; set; }
        public string ClassName { get; set; }             
    }

}
