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
    
    public partial class ReportRunLog
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public Nullable<long> AccountId { get; set; }
        public string Ip { get; set; }
        public System.DateTime RunStartTime { get; set; }
        public bool RunNow { get; set; }
    }
}