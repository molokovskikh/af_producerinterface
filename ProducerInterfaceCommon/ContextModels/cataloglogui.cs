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
    
    public partial class cataloglogui
    {
        public long Id { get; set; }
        public long NameId { get; set; }
        public System.DateTime LogTime { get; set; }
        public long UserId { get; set; }
        public string OperatorHost { get; set; }
        public long ObjectReference { get; set; }
        public string ObjectReferenceNameUi { get; set; }
        public int Type { get; set; }
        public string PropertyName { get; set; }
        public string PropertyNameUi { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public bool Apply { get; set; }
        public Nullable<long> AdminId { get; set; }
        public string AdminName { get; set; }
        public Nullable<System.DateTime> DateEdit { get; set; }
        public string Login { get; set; }
        public string UserName { get; set; }
        public Nullable<long> ProducerId { get; set; }
        public string ProducerName { get; set; }
    }
}
