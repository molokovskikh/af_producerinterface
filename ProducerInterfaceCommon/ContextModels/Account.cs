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
    
    public partial class Account
    {
        public Account()
        {
            this.AccountEmail = new HashSet<AccountEmail>();
            this.AccountGroup = new HashSet<AccountGroup>();
            this.promotions = new HashSet<promotions>();
        }
    
        public long Id { get; set; }
        public string Login { get; set; }
        public sbyte TypeUser { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Appointment { get; set; }
        public Nullable<long> CompanyId { get; set; }
        public Nullable<System.DateTime> PasswordUpdated { get; set; }
        public Nullable<sbyte> Enabled { get; set; }
        public string Phone { get; set; }
    
        public virtual AccountCompany AccountCompany { get; set; }
        public virtual ICollection<AccountEmail> AccountEmail { get; set; }
        public virtual ICollection<AccountGroup> AccountGroup { get; set; }
        public virtual ICollection<promotions> promotions { get; set; }
    }
}
