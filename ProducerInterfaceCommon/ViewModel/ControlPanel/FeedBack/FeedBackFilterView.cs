using System;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.FeedBack
{
    public class FeedBackList
    {
        public List<FeedBackItem> FeedList { get; set; }
        public List<Paginator> PaginatorLinks { get; set; }

        public int PageIndex { get; set; }

        public string SortTime { get; set; }
        public string SortProducerName { get; set; }
        public string SortAccountName { get; set; }
        public string SortType { get; set; }
        public string SortStatus { get; set; }
    }

    public class FeedBackItemSelect : FeedBackItem
    {
        //public List<FeedBackComment> Comments{get;set;}
        public int FeedStatusId { get; set; }
        public List<OptionElement> StatusList { get; set; }
    }

    //public class FeedBackComment
    //{
    //    public int Id { get; set; }
    //    public string Description { get; set; }
    //    public DateTime DateAdd { get; set; }
    //    public long IdAccount { get; set; }
    //    public string AdminName { get; set; }
    //}

    public class FeedBackItem
    {
        public long Id { get; set; }
      
        public string Description { get; set; }

        public DateTime CreateDate { get; set; }
        
        public string DateTimeString
        { get { return this.CreateDate.ToString("dd.MM.yyyy"); } }
        
        private int Status { get; set; }        
     
        public int TypeFeedBack { get; set; }       

        public string TypeString
        { get { return AttributeHelper.DisplayName((FeedBackTypePrivate)TypeFeedBack); } }        

        public long? AccountId { get; set; }

        public string AccountLogin { get; set; }      
        public string Producername { get; set; }

        public string UrlString { get; set; }
        public string About { get; set; }

        public string Message { get; set; }

        public Enums.FeedBackStatus FeedBackStatus
        { get { return (Enums.FeedBackStatus)Status; } set { Status = (int)value; } }
        public string StatusString
        { get{ return AttributeHelper.DisplayName((Enums.FeedBackStatus)Status); } }    
    }

    // данная модель будет возвращатся при поиске результатов с клиента
    public class FeedBackFilter
    {    
        public string DateBegin { get; set; }    
        public string DateEnd { get; set; }
        public int PageIndex { get; set; }
        public long ProducerId { get; set; }
        public long AccountId { get; set; }
        public int PageCount { get; set; }

        public List<OptionElement> ProducerList { get; set; }
        public List<OptionElement> AccountList { get; set; }
        public List<OptionElement> PageCountList { get; set;}       
    }
       
    public class Paginator
    {
        public int Counter { get; set; }
        public int ViewCounter { get; set; }
        public string ClassName { get; set; }             
    }

  

   

 
  
}
