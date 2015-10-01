using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Models;
using AnalitFramefork.Components.Validation;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Util;
using NHibernate.Validator.Engine;

namespace AnalitFramefork.Hibernate.Models
{
	/// <summary>
	/// Модель для формы для редактирования параметров шаблона производителя
	/// </summary>
	public class ProducerRatingReportTemplateForm : ReportTemplateForm
	{
		public IList<Drug> AvailableDrugs { get; set; }

		[ValidatorNotEmpty]
		public IList<Drug> IncludedDrugs { get; set; }

		public IList<Region> AvailableRegions { get; set; }

		[ValidatorNotEmpty]
		public IList<Region> IncludedRegions { get; set; }

		public IList<Supplier> AvailableSuppliers { get; set; }

		[ValidatorNotEmpty]
		public IList<Supplier> ExcludedSuppliers { get; set; }

		public ISession DbSession { get; set; }

		/// <summary>
		/// Когда создается отчет изначально, у него еще нет подотчетов и параметров.
		/// Соответственно, чтобы добраться для значений параметров нужно проделать долгий путь.
		/// Чтобы не париться с этим вопросом, мы сначала анализируем законченность шаблона и выставляем данный флаг
		/// </summary>
		[Description("Флаг того, что пользователь завершил все этапы создания шаблона")]
		protected bool TemplateComplete = false;

		public ProducerRatingReportTemplateForm(ReportTemplate template, ISession session) : base(template, session)
		{
			DbSession = session;
			if (ReportTemplate.GeneralReport.Reports.Any())
				TemplateComplete = true;

			IncludedRegions = GetIncludedRegions();
			ExcludedSuppliers = GetExcludedSuppliers();
			IncludedDrugs = GetIncludedDrugs();
			Refresh();
		}

		private IList<Drug> GetIncludedDrugs()
		{
			if (!TemplateComplete)
				return new List<Drug>();
			var subreport = ReportTemplate.GeneralReport.Reports.First();
			var excludeProducersProperty = subreport.Type.Properties.First(i => i.Name == "RegionEqual");
			var propvalue = subreport.Properties.FirstOrDefault(i => i.Property == excludeProducersProperty);
			var values = propvalue.Values;
			var intvalues = values.Select(i => int.Parse(i.Value)).ToArray();
			var drugs = DbSession.Query<Drug>().Where(i => intvalues.Contains(i.Id)).ToList();
			return drugs;
		}

		private IList<Region> GetIncludedRegions()
		{
			if (!TemplateComplete)
				return new List<Region>();
			var subreport = ReportTemplate.GeneralReport.Reports.First();
			var excludeProducersProperty = subreport.Type.Properties.First(i => i.Name == "RegionEqual");
			var propvalue = subreport.Properties.FirstOrDefault(i => i.Property == excludeProducersProperty);
			var values = propvalue.Values;
			var intvalues = values.Select(i => ulong.Parse(i.Value)).ToArray();
			var regions = DbSession.Query<Region>().Where(i => intvalues.Contains(i.Id)).ToList();
			return regions;
		}

		private IList<Supplier> GetExcludedSuppliers()
		{
			if (!TemplateComplete)
				return new List<Supplier>();

			var subreport = ReportTemplate.GeneralReport.Reports.First();
			var excludeProducersProperty  = subreport.Type.Properties.First(i => i.Name == "FirmCodeNonEqual");
			var propvalue = subreport.Properties.FirstOrDefault(i => i.Property == excludeProducersProperty);
			var values = propvalue.Values;
			var intvalues = values.Select(i => int.Parse(i.Value)).ToArray();
			var suppliers = DbSession.Query<Supplier>().Where(i => intvalues.Contains(i.Id)).ToList();
			return suppliers;
		}

		public override InvalidValue[] Validate(ISession session)
		{
			var list = new List<InvalidValue>();
			return list.ToArray();
		}

		public static IList<Supplier> FindSuppliers(ISession session, string name, IList<Region> includedRegions, IList<Supplier> excludedSuppliers)
		{
			var suppliers = session.Query<Supplier>().Where(i => i.Disabled == false && !i.Name.Contains("иатриц") && !i.Name.Contains("ассортимент") && i.Name.Contains(name)).ToList();
			var filtered = suppliers.Where(i => i.PriceLists.Any(p => p.Enabled && !p.IsLocal && p.Type != PriceType.Assortment) && includedRegions.Intersect(i.Regions).Any()).ToList();
			filtered = filtered.Where(i => !excludedSuppliers.Contains(i)).ToList();
			return filtered;
		}

		public static IList<Region> FindRegions(ISession session, string name, IList<Region> excludedRegions)
		{
			var regions = session.Query<Region>().Where(i => i.Name != "Inforoom" && i.Title.Contains(name) && !i.DrugSearchReference).ToList();
			var filtered = regions.Where(i => !excludedRegions.Contains(i)).ToList();
			return filtered;
		}

		public static IList<Drug> FindDrugs(ISession session, string name, Producer producer, IList<Drug> excludedDrugs)
		{
			var drugs = producer.Drugs.ToList();
			var filtered = drugs.Where(i => !excludedDrugs.Contains(i) && i.Name.ToLower().Contains(name.ToLower())).ToList();
			return filtered;
		}

		public override void Refresh()
		{
			AvailableDrugs = FindDrugs(DbSession,"",ReportTemplate.User.Producer,IncludedDrugs);
			AvailableRegions = FindRegions(DbSession, "", IncludedRegions);
			AvailableSuppliers = FindSuppliers(DbSession, "", IncludedRegions, ExcludedSuppliers);
		}

		public override void Save(ISession session)
		{
			PrepareReport();
			ReplaceReportListProperty("FirmCodeNonEqual", ExcludedSuppliers);
			ReplaceReportListProperty("RegionEqual", IncludedRegions);
			ReplaceReportListProperty("FullNameEqual", IncludedDrugs);
			ReportTemplate.GeneralReport.Reports.First().Enabled = true;
			DbSession.Save(ReportTemplate);
		}

		private void PrepareReport()
		{
			Report subreport;
			if (!ReportTemplate.GeneralReport.Reports.Any())
			{
				subreport = new Report();
				var type = DbSession.Query<ReportType>().First(i => i.Id == 6);
				subreport.Type = type;
				subreport.GeneralReport = ReportTemplate.GeneralReport;
				subreport.Name = "Отчет пользователя " + ReportTemplate.User.Email + ", поставщика " +
				                 ReportTemplate.User.Producer.Name;
				ReportTemplate.GeneralReport.Reports.Add(subreport);
			}
			else
				subreport = ReportTemplate.GeneralReport.Reports.First();
			DbSession.Save(subreport);
			DbSession.Flush();
			DbSession.Refresh(subreport);
			//Дефолтные параметры для всех отчетов
			//Они создаются триггером в БД, поэтому и делаем Flush + Refresh
			ReportPropertyValue prop;
			prop = PrepareParamValue(subreport, "JunkState");
			prop.Value = "0";
			prop = PrepareParamValue(subreport, "BuildChart");
			prop.Value = "0";
			prop = PrepareParamValue(subreport, "DoNotShowAbsoluteValues");
			prop.Value = "0";
			prop = PrepareParamValue(subreport, "ByPreviousMonth");
			prop.Value = "1";
			prop = PrepareParamValue(subreport, "ReportInterval");
			prop.Value = "27";

			//Позиции колонок
			prop = PrepareParamValue(subreport, "FullNamePosition");
			prop.Value = "0";
			prop = PrepareParamValue(subreport, "FirmCrPosition");
			prop.Value = "2";
			prop = PrepareParamValue(subreport, "RegionPosition");
			prop.Value = "3";
			prop = PrepareParamValue(subreport, "FirmCodePosition");
			prop.Value = "4";

			//Списочные параметры необходимые рейтинговому отчету
			//Их значения задаются пользователем при помощи как раз данной формы
			PrepareParamValue(subreport, "FirmCodeNonEqual");
			PrepareParamValue(subreport, "RegionEqual");
			PrepareParamValue(subreport, "FullNameEqual");
		}

		private ReportPropertyValue PrepareParamValue(Report subreport, string propname)
		{
			var typeProperty = subreport.Type.Properties.First(i => i.Name == propname);
			var propvalue = subreport.Properties.FirstOrDefault(i => i.Property == typeProperty);
			if (propvalue == null)
			{
				propvalue = new ReportPropertyValue();
				propvalue.Property = typeProperty;
				propvalue.Value = "1";
				propvalue.Report = subreport;
				subreport.Properties.Add(propvalue);
				DbSession.Save(propvalue);
			}
			return propvalue;
		}

		private void ReplaceReportListProperty(string propname, IEnumerable models)
		{
			var subreport = ReportTemplate.GeneralReport.Reports.First();
			var typeProperty = subreport.Type.Properties.First(i => i.Name == propname);
			var propvalue = subreport.Properties.FirstOrDefault(i => i.Property == typeProperty);
			propvalue.Values.Clear();
			foreach (var model in models)
			{
				var basemodel = model as BaseModel;
				var listvalue = new ReportPropertyListValue();
				listvalue.Value = basemodel.GetId();
				listvalue.Property = propvalue;
				propvalue.Values.Add(listvalue);
			}
		}
	}
}