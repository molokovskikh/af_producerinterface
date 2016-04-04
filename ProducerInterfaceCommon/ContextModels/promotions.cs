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
    
    public partial class promotions
    {
        public promotions()
        {
            this.promotionToDrug = new HashSet<promotionToDrug>();
            this.PromotionsToSupplier = new HashSet<PromotionsToSupplier>();
        }
    
        public long Id { get; set; }
        public System.DateTime UpdateTime { get; set; }
        public bool Enabled { get; set; }
        public Nullable<long> AdminId { get; set; }
        public long ProducerId { get; set; }
        public long ProducerUserId { get; set; }
        public string Annotation { get; set; }
        public string PromoFile { get; set; }
        public Nullable<int> PromoFileId { get; set; }
        public bool AgencyDisabled { get; set; }
        public string Name { get; set; }
        public decimal RegionMask { get; set; }
        public Nullable<System.DateTime> Begin { get; set; }
        public Nullable<System.DateTime> End { get; set; }
        public bool Status { get; set; }
        public Nullable<long> ProducerAdminId { get; set; }
        public Nullable<bool> AllSuppliers { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Account Account1 { get; set; }
        public virtual MediaFiles MediaFiles { get; set; }
        public virtual ICollection<promotionToDrug> promotionToDrug { get; set; }
        public virtual ICollection<PromotionsToSupplier> PromotionsToSupplier { get; set; }
    }
}
