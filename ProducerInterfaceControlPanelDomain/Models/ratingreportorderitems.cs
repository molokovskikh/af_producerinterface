//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterfaceControlPanelDomain.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ratingreportorderitems
    {
        public long CatalogId { get; set; }
        public decimal RegionCode { get; set; }
        public long SupplierId { get; set; }
        public long ProducerId { get; set; }
        public Nullable<long> PharmacyId { get; set; }
        public Nullable<long> Quantity { get; set; }
        public long OrderId { get; set; }
        public Nullable<long> AddressId { get; set; }
        public bool IsLocal { get; set; }
        public System.DateTime WriteTime { get; set; }
    }
}
