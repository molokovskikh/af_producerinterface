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
    
    public partial class reportrunlogwithuser
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string Ip { get; set; }
        public System.DateTime RunStartTime { get; set; }
        public bool RunNow { get; set; }
        public string UserName { get; set; }
        public string ProducerName { get; set; }
        public string MailTo { get; set; }
    }
}
