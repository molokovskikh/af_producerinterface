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
    
    public partial class ProducerUser
    {
        public ProducerUser()
        {
            this.DrugDescriptionRemark = new HashSet<DrugDescriptionRemark>();
            this.promotions = new HashSet<promotions>();
            this.user_logs = new HashSet<user_logs>();
            this.usertouserrole = new HashSet<usertouserrole>();
            this.promotions1 = new HashSet<promotions>();
            this.user_logs1 = new HashSet<user_logs>();
            this.usertouserrole1 = new HashSet<usertouserrole>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Appointment { get; set; }
        public Nullable<long> ProducerId { get; set; }
        public Nullable<System.DateTime> PasswordUpdated { get; set; }
        public sbyte PasswordToUpdate { get; set; }
        public sbyte Enabled { get; set; }
    
        public virtual ICollection<DrugDescriptionRemark> DrugDescriptionRemark { get; set; }
        public virtual ICollection<promotions> promotions { get; set; }
        public virtual ICollection<user_logs> user_logs { get; set; }
        public virtual ICollection<usertouserrole> usertouserrole { get; set; }
        public virtual ICollection<promotions> promotions1 { get; set; }
        public virtual ICollection<user_logs> user_logs1 { get; set; }
        public virtual ICollection<usertouserrole> usertouserrole1 { get; set; }
    }
}
