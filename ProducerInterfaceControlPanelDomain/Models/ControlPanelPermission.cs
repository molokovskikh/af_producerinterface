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
    
    public partial class ControlPanelPermission
    {
        public ControlPanelPermission()
        {
            this.ControlPanelGroup = new HashSet<ControlPanelGroup>();
        }
    
        public long Id { get; set; }
        public string ControllerAction { get; set; }
        public string ActionAttributes { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }    

        public virtual ICollection<ControlPanelGroup> ControlPanelGroup { get; set; }
    }
}
