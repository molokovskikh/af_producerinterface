﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class catalogsEntities : DbContext
    {
        public catalogsEntities()
            : base("name=catalogsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Catalog> Catalog { get; set; }
        public virtual DbSet<catalogforms> catalogforms { get; set; }
        public virtual DbSet<catalognames> catalognames { get; set; }
        public virtual DbSet<Descriptions> Descriptions { get; set; }
        public virtual DbSet<mnn> mnn { get; set; }
        public virtual DbSet<assortment> assortment { get; set; }
        public virtual DbSet<Producers> Producers { get; set; }
        public virtual DbSet<Products> Products { get; set; }
    }
}
