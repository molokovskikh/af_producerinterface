//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterfaceCommon.ContextModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class AccountFeedBack
    {
        public AccountFeedBack()
        {
            this.AccountFeedBackComment = new HashSet<AccountFeedBackComment>();
        }
    
        public long Id { get; set; }
        public sbyte Status { get; set; }
        public string Description { get; set; }
        public System.DateTime DateAdd { get; set; }
        public Nullable<long> AccountId { get; set; }
        public string UrlString { get; set; }
        public sbyte Type { get; set; }
        public string Contacts { get; set; }
        public Nullable<long> AdminId { get; set; }
        public Nullable<System.DateTime> DateEdit { get; set; }
    
        public virtual ICollection<AccountFeedBackComment> AccountFeedBackComment { get; set; }
        public virtual Account Account { get; set; }
        public virtual Account Account1 { get; set; }
    }
}
