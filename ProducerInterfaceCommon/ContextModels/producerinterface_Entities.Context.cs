﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class producerinterface_Entities : DbContext
    {
        public producerinterface_Entities()
            : base("name=producerinterface_Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<DrugDescriptionRemark> DrugDescriptionRemark { get; set; }
        public DbSet<jobextend> jobextend { get; set; }
        public DbSet<mailform> mailform { get; set; }
        public DbSet<profilenews> profilenews { get; set; }
        public DbSet<promotionToDrug> promotionToDrug { get; set; }
        public DbSet<reporttemplate> reporttemplate { get; set; }
        public DbSet<reportxml> reportxml { get; set; }
        public DbSet<user_logs> user_logs { get; set; }
        public DbSet<assortment> assortment { get; set; }
        public DbSet<catalognames> catalognames { get; set; }
        public DbSet<drugdescription> drugdescription { get; set; }
        public DbSet<drugfamily> drugfamily { get; set; }
        public DbSet<drugformproducer> drugformproducer { get; set; }
        public DbSet<drugmnn> drugmnn { get; set; }
        public DbSet<jobextendwithproducer> jobextendwithproducer { get; set; }
        public DbSet<mailformwithfooter> mailformwithfooter { get; set; }
        public DbSet<pharmacynames> pharmacynames { get; set; }
        public DbSet<producernames> producernames { get; set; }
        public DbSet<ratingreportorderitems> ratingreportorderitems { get; set; }
        public DbSet<regionnames> regionnames { get; set; }
        public DbSet<suppliernames> suppliernames { get; set; }
        public DbSet<LogForNet> LogForNet { get; set; }
        public DbSet<drugfamilynames> drugfamilynames { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<AccountCompany> AccountCompany { get; set; }
        public DbSet<AccountEmail> AccountEmail { get; set; }
        public DbSet<AccountGroup> AccountGroup { get; set; }
        public DbSet<AccountPermission> AccountPermission { get; set; }
        public DbSet<CompanyDomainName> CompanyDomainName { get; set; }
        public DbSet<usernames> usernames { get; set; }
        public DbSet<promotions> promotions { get; set; }
    }
}
