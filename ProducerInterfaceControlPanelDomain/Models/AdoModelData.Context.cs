﻿//------------------------------------------------------------------------------
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
    
        public DbSet<jobextend> jobextend { get; set; }
        public DbSet<mailform> mailform { get; set; }
        public DbSet<producers> producers { get; set; }
        public DbSet<ProducerUser> ProducerUser { get; set; }
        public DbSet<promotions> promotions { get; set; }
        public DbSet<promotionToDrug> promotionToDrug { get; set; }
        public DbSet<reporttemplate> reporttemplate { get; set; }
        public DbSet<reportxml> reportxml { get; set; }
        public DbSet<user_logs> user_logs { get; set; }
        public DbSet<UserPermission> UserPermission { get; set; }
        public DbSet<userrole> userrole { get; set; }
        public DbSet<usertouserrole> usertouserrole { get; set; }
        public DbSet<userpermissionrole> userpermissionrole { get; set; }
        public DbSet<assortment> assortment { get; set; }
        public DbSet<catalognames> catalognames { get; set; }
        public DbSet<drugfamily> drugfamily { get; set; }
        public DbSet<mailformwithfooter> mailformwithfooter { get; set; }
        public DbSet<pharmacynames> pharmacynames { get; set; }
        public DbSet<producernames> producernames { get; set; }
        public DbSet<ratingreportorderitems> ratingreportorderitems { get; set; }
        public DbSet<regionnames> regionnames { get; set; }
        public DbSet<suppliernames> suppliernames { get; set; }
        public DbSet<usernames> usernames { get; set; }
        public DbSet<jobextendwithproducer> jobextendwithproducer { get; set; }
        public DbSet<profilenews> profilenews { get; set; }
        public DbSet<ControlPanelGroup> ControlPanelGroup { get; set; }
        public DbSet<ControlPanelPermission> ControlPanelPermission { get; set; }
        public DbSet<ControlPanelUser> ControlPanelUser { get; set; }
        public DbSet<controlpaneluserpermission> controlpaneluserpermission { get; set; }
        public DbSet<produceruserpermission> produceruserpermission { get; set; }
        public DbSet<drugformproducer> drugformproducer { get; set; }
    }
}
