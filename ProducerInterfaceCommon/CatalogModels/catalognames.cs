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
    
    public partial class catalognames
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public catalognames()
        {
            this.Catalog = new HashSet<Catalog>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public Nullable<long> DescriptionId { get; set; }
        public Nullable<long> MnnId { get; set; }
        public System.DateTime UpdateTime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Catalog> Catalog { get; set; }
    }
}
