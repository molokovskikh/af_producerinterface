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
    
    public partial class producers
    {
        public producers()
        {
            this.profilenews = new HashSet<profilenews>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<profilenews> profilenews { get; set; }
    }
}
