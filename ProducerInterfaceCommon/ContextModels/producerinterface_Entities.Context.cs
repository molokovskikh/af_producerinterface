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
	using System.Data.Entity.Core.Objects;
	using System.Data.Entity.Infrastructure;
	using System.Linq;

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
    
        public DbSet<mailform> mailform { get; set; }
        public DbSet<profilenews> profilenews { get; set; }
        public DbSet<promotionToDrug> promotionToDrug { get; set; }
        public DbSet<reporttemplate> reporttemplate { get; set; }
        public DbSet<reportxml> reportxml { get; set; }
        public DbSet<user_logs> user_logs { get; set; }
        public DbSet<assortment> assortment { get; set; }
        public DbSet<catalognames> catalognames { get; set; }
        public DbSet<drugfamily> drugfamily { get; set; }
        public DbSet<pharmacynames> pharmacynames { get; set; }
        public DbSet<producernames> producernames { get; set; }
        public DbSet<ratingreportorderitems> ratingreportorderitems { get; set; }
        public DbSet<regionnames> regionnames { get; set; }
        public DbSet<suppliernames> suppliernames { get; set; }
        public DbSet<LogForNet> LogForNet { get; set; }
        public DbSet<drugfamilynames> drugfamilynames { get; set; }
        public DbSet<AccountCompany> AccountCompany { get; set; }
        public DbSet<CompanyDomainName> CompanyDomainName { get; set; }
        public DbSet<usernames> usernames { get; set; }
        public DbSet<DrugDescriptionRemark> DrugDescriptionRemark { get; set; }
        public DbSet<NotificationToProducers> NotificationToProducers { get; set; }
        public DbSet<catalognameswithuptime> catalognameswithuptime { get; set; }
        public DbSet<ReportRunLog> ReportRunLog { get; set; }
        public DbSet<reportrunlogwithuser> reportrunlogwithuser { get; set; }
        public DbSet<mailformwithfooter> mailformwithfooter { get; set; }
        public DbSet<AccountGroup> AccountGroup { get; set; }
        public DbSet<AccountPermission> AccountPermission { get; set; }
        public DbSet<jobextendwithproducer> jobextendwithproducer { get; set; }
        public DbSet<jobextend> jobextend { get; set; }
        public DbSet<NewsChange> NewsChange { get; set; }
        public DbSet<AccountAppointment> AccountAppointment { get; set; }
        public DbSet<MediaFiles> MediaFiles { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<ReportDescription> ReportDescription { get; set; }
        public DbSet<PromotionsToSupplier> PromotionsToSupplier { get; set; }
        public DbSet<supplierregions> supplierregions { get; set; }
        public DbSet<AccountEmail> AccountEmail { get; set; }
        public DbSet<promotions> promotions { get; set; }
        public DbSet<ReportRegion> ReportRegion { get; set; }
        public DbSet<regionsnamesleaftoreport> regionsnamesleaftoreport { get; set; }
        public DbSet<regionsnamesleaf> regionsnamesleaf { get; set; }
        public DbSet<AccountFeedBack> AccountFeedBack { get; set; }
        public DbSet<feedbackui> feedbackui { get; set; }
        public DbSet<DescriptionLog> DescriptionLog { get; set; }
    
        public virtual ObjectResult<PromotionsInRegionMask_Result> PromotionsInRegionMask(Nullable<long> rGM)
        {
            var rGMParameter = rGM.HasValue ?
                new ObjectParameter("RGM", rGM) :
                new ObjectParameter("RGM", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<PromotionsInRegionMask_Result>("PromotionsInRegionMask", rGMParameter);
        }
    }
}
