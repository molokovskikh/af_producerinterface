//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityContext.ContextModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class user_logs
    {
        public long Id { get; set; }
        public Nullable<long> ProducerUserId { get; set; }
        public Nullable<long> ModelId { get; set; }
        public string ModelClass { get; set; }
        public System.DateTime Date { get; set; }
        public sbyte Type { get; set; }
        public string Message { get; set; }
    
        public virtual ProducerUser ProducerUser { get; set; }
    }
}
