﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProducerInterfaceCommon.LoggerModels
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<LogChangeSet> LogChangeSet { get; set; }
        public virtual DbSet<LogObjectChange> LogObjectChange { get; set; }
        public virtual DbSet<LogPropertyChange> LogPropertyChange { get; set; }
        public virtual DbSet<logchangeview> logchangeview { get; set; }
        public virtual DbSet<propertychangeview> propertychangeview { get; set; }
    }
}
