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
    
    public partial class AccountFeedBackComment
    {
        public int Id { get; set; }
        public long IdFeedBack { get; set; }
        public string Comment { get; set; }
        public System.DateTime DateAdd { get; set; }
        public long AdminId { get; set; }
    
        public virtual AccountFeedBack AccountFeedBack { get; set; }
        public virtual Account Account { get; set; }
    }
}
