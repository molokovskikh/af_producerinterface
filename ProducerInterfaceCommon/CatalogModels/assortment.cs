//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterfaceCommon.CatalogModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class assortment
    {
        public long Id { get; set; }
        public long CatalogId { get; set; }
        public long ProducerId { get; set; }
        public bool Checked { get; set; }
    
        public virtual Catalog Catalog { get; set; }
        public virtual Producers Producers { get; set; }
    }
}
