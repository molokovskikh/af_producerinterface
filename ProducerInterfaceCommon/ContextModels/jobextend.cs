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

    public partial class jobextend
    {
        public string SchedName { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string CustomName { get; set; }
        public string Scheduler { get; set; }
        public int ReportType { get; set; }
        public long? ProducerId { get; set; }
        public long CreatorId { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastModified { get; set; }
        public int DisplayStatus { get; set; }
        public Nullable<System.DateTime> LastRun { get; set; }
        public bool Enable { get; set; }
    }
}
