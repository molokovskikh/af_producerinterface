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
    
    public partial class NewsChange
    {
        public long IdAccount { get; set; }
        public long IdNews { get; set; }
        public byte TypeCnhange { get; set; }
        public System.DateTime DateChange { get; set; }
        public string NewsNewTema { get; set; }
        public string NewsOldTema { get; set; }
        public string NewsOldDescription { get; set; }
        public string NewsNewDescription { get; set; }
        public long Id { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual NotificationToProducers NotificationToProducers { get; set; }
    }
}