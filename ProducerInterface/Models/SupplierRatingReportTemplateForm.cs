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
	/// Модель для формы для редактирования параметров шаблона рейтингового отчета по поставщикам.
	/// По параметрам отличается от рейтингового отчета по аптекам тем, что тут добавляется список поставщиков, которых необходимо исключить из выборки.
	/// </summary>
	public class SupplierRatingReportTemplateForm : DrugstoreRatingReportTemplateForm
	{
		[Description("Список поставщиков доступных для исключения из отчета")]
        public IList<Supplier> AvailableSuppliers { get; set; }

		[Description("Список поставщиков исключенных из отчета"), ValidatorNotEmpty]
		public IList<Supplier> ExcludedSuppliers { get; set; }

		public SupplierRatingReportTemplateForm(ReportTemplate template, ISession session) : base(template, session)
		{
		}

		protected override void Initialize()
		{
			ExcludedSuppliers = GetExcludedSuppliers();
			base.Initialize();
		}

		/// <summary>
		/// Получение списка поставщиков, которых пользователь добавил в исключения
		/// </summary>
		/// <returns></returns>
		protected IList<Supplier> GetExcludedSuppliers()
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

		/// <summary>
		/// Поиск поставщиков для рейтингового отчета
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="pattern">Шаблон имени поставщика (используется для поиска). Можно заменить на пустую строку.</param>
		/// <param name="includedRegions">Список регионов в которых нужно искать поставщиков</param>
		/// <param name="excludedSuppliers">Список моделей, которые необходимо исключить из выборки (обычно это те поставщики, который пользователь уже добавил на форму)</param>
		/// <returns></returns>
		public static IList<Supplier> FindSuppliers(ISession session, string pattern, IList<Region> includedRegions, IList<Supplier> excludedSuppliers)
		{
			var suppliers = session.Query<Supplier>().Where(i => i.Disabled == false && !i.Name.Contains("иатриц") && !i.Name.Contains("ассортимент") && i.Name.Contains(pattern)).ToList();
			var filtered = suppliers.Where(i => i.PriceLists.Any(p => p.Enabled && !p.IsLocal && p.Type != PriceType.Assortment) && includedRegions.Intersect(i.Regions).Any()).ToList();
			filtered = filtered.Where(i => !excludedSuppliers.Contains(i)).ToList();
			return filtered;
		}

		/// <summary>
		/// Обновление данных в форме.
		/// </summary>
		public override void Refresh()
		{
			base.Refresh();
			AvailableSuppliers = FindSuppliers(DbSession, "", IncludedRegions, ExcludedSuppliers);
		}

		/// <summary>
		/// Приведение отчета к виду, готовому для запуска (добавление в него параметров по умолчанию и т.д.).
		/// </summary>
		protected override void PrepareReport()
		{
			base.PrepareReport();
			var report = ReportTemplate.GeneralReport.Reports.First();
			PrepareParamValue(report, "FirmCodeNonEqual");
			ReplaceReportListProperty(report, "FirmCodeNonEqual", ExcludedSuppliers);
		}
	}
}