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
            this.AccountFeedBack = new HashSet<AccountFeedBack>();
            this.promotions = new HashSet<promotions>();
            this.promotions1 = new HashSet<promotions>();
            this.AccountAppointment1 = new HashSet<AccountAppointment>();
            this.NewsChange = new HashSet<NewsChange>();
            this.AccountGroup = new HashSet<AccountGroup>();
        }
    
        public long Id { get; set; }
        public string Login { get; set; }
        public Nullable<int> AppointmentId { get; set; }
        public sbyte TypeUser { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string OtherName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Appointment { get; set; }
        public Nullable<long> CompanyId { get; set; }
        public Nullable<System.DateTime> PasswordUpdated { get; set; }
        public Nullable<sbyte> Enabled { get; set; }
        public Nullable<System.DateTime> LastUpdatePermisison { get; set; }
        public Nullable<decimal> RegionMask { get; set; }
        public Nullable<System.DateTime> SecureTime { get; set; }
    
        public virtual AccountAppointment AccountAppointment { get; set; }
        public virtual AccountCompany AccountCompany { get; set; }
        public virtual ICollection<AccountEmail> AccountEmail { get; set; }
        public virtual ICollection<AccountFeedBack> AccountFeedBack { get; set; }
        public virtual ICollection<promotions> promotions { get; set; }
        public virtual ICollection<promotions> promotions1 { get; set; }
        public virtual ICollection<AccountAppointment> AccountAppointment1 { get; set; }
        public virtual ICollection<NewsChange> NewsChange { get; set; }
        public virtual ICollection<AccountGroup> AccountGroup { get; set; }
    }
}
