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
	/// Модель для формы для редактирования параметров шаблона для рейтингового отчета по аптекам
	/// </summary>
	public class DrugstoreRatingReportTemplateForm : ReportTemplateForm
	{
		[Description("Список препаратов доступных для добавления в отчет")]
		public IList<Drug> AvailableDrugs { get; set; }

		[Description("Список препаратов добавленных в отчет"),ValidatorNotEmpty]
		public IList<Drug> IncludedDrugs { get; set; }

		[Description("Список регионов доступных для добавления в отчет")]
		public IList<Region> AvailableRegions { get; set; }

		[Description("Список регионов добавленных в отчет"),ValidatorNotEmpty]
		public IList<Region> IncludedRegions { get; set; }

		public DrugstoreRatingReportTemplateForm(ReportTemplate template, ISession session) : base(template, session)
		{
		}

		protected override void Initialize()
		{
			IncludedRegions = GetIncludedRegions();
			IncludedDrugs = GetIncludedDrugs();
			Refresh();
		}
		/// <summary>
		/// Получение списка препаратов, которые пользователь уже добавил на форму
		/// </summary>
		/// <returns></returns>
		protected IList<Drug> GetIncludedDrugs()
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

		/// <summary>
		/// Получение списка регионов, которые пользователь уже добавил на форму
		/// </summary>
		/// <returns></returns>
		protected IList<Region> GetIncludedRegions()
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

		/// <summary>
		/// Валидация модели на наличие ошибок
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		/// <returns></returns>
		public override InvalidValue[] Validate(ISession session)
		{
			var list = new List<InvalidValue>();
			return list.ToArray();
		}

		/// <summary>
		/// Обновление списочных данных формы.
		/// </summary>
		public override void Refresh()
		{
			AvailableDrugs = FindDrugs(DbSession,"",ReportTemplate.User.Producer,IncludedDrugs);
			AvailableRegions = FindRegions(DbSession, "", IncludedRegions);
		}

		/// <summary>
		/// Сохранение отчета в базу данных
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		public override void Save(ISession session)
		{
			//В целом, сессия уже есть в отчете. 
			//Она является параметром только для того, чтобы программист понимал, что данные будут сохранены в сессию.
			PrepareReport();
			var subreport = ReportTemplate.GeneralReport.Reports.First();
			ReplaceReportListProperty(subreport,"RegionEqual", IncludedRegions);
			ReplaceReportListProperty(subreport,"FullNameEqual", IncludedDrugs);
			ReportTemplate.GeneralReport.Reports.First().Enabled = true;
			DbSession.Save(ReportTemplate);
		}

		protected virtual void PrepareReport()
		{
			ReportTemplate.GeneralReport.ReportFileName = "";
			DbSession.Save(ReportTemplate.GeneralReport);

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
				DbSession.Save(subreport);
				DbSession.Flush();
				DbSession.Refresh(subreport);
			}
			else
				subreport = ReportTemplate.GeneralReport.Reports.First();
			
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

		/// <summary>
		/// Функция получения или создания параметра для отчета
		/// </summary>
		/// <param name="subreport">Отчет, у которого необходимо получить или создать параметр</param>
		/// <param name="propname">Название параметра</param>
		/// <returns></returns>
		protected ReportPropertyValue PrepareParamValue(Report subreport, string propname)
		{
			var typeProperty = subreport.Type.Properties.FirstOrDefault(i => i.Name == propname);
			if (typeProperty == null)
				throw new Exception(string.Format("Не удалось получить параметр \"{0}\" для отчета типа \"{1}\".", propname,subreport.Type.Name));
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

		/// <summary>
		/// Создание значений для параметров списочного типа для отчета
		/// </summary>
		/// <param name="subreport">Отчет</param>
		/// <param name="propname">Имя параметра</param>
		/// <param name="models">Список моделей, который необходимо внести в качестве значений</param>
		protected void ReplaceReportListProperty(Report subreport, string propname, IEnumerable models)
		{
			
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

		/// <summary>
		/// Поиск регионов для рейтиговых отчетов
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="pattern">Шаблон имени региона (используется для поиска). Можно заменить на пустую строку.</param>
		/// <param name="excludedRegions">Список моделей, которые необходимо исключить из выборки (обычно это те регионы, который пользователь уже добавил на форму)</param>
		/// <returns></returns>
		public static IList<Region> FindRegions(ISession session, string pattern, IList<Region> excludedRegions)
		{
			var regions = session.Query<Region>().Where(i => i.Name != "Inforoom" && i.Title.Contains(pattern) && !i.DrugSearchReference).ToList();
			var filtered = regions.Where(i => !excludedRegions.Contains(i)).ToList();
			return filtered;
		}

		/// <summary>
		/// Поиск препаратов для рейтинговых отчетов
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="pattern">Шаблон имени препарата (используется для поиска). Можно заменить на пустую строку.</param>
		/// <param name="producer">Производитель у которого необходимо искать препараты</param>
		/// <param name="excludedDrugs">Список моделей, которые необходимо исключить из выборки (обычно это те препараты, который пользователь уже добавил на форму)</param>
		/// <returns></returns>
		public static IList<Drug> FindDrugs(ISession session, string pattern, Producer producer, IList<Drug> excludedDrugs)
		{
			var drugs = producer.Drugs.ToList();
			var filtered = drugs.Where(i => !excludedDrugs.Contains(i) && i.Name.ToLower().Contains(pattern.ToLower())).ToList();
			return filtered;
		}
	}
}