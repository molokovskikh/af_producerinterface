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
    
    public partial class LogForNet
    {
        public int Id { get; set; }
        public System.DateTime Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string App { get; set; }
    }
}
