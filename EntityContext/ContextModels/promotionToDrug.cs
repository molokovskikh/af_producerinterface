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
    
    public partial class promotionToDrug
    {
        public long DrugId { get; set; }
        public long PromotionId { get; set; }
    
        public virtual promotions promotions { get; set; }
    }
}
