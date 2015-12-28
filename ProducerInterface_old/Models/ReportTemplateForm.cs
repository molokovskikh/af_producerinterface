using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AnalitFramefork.Components.Models;
using AnalitFramefork.Components.Validation;
using NHibernate;
using NHibernate.Event;
using NHibernate.Mapping;
using NHibernate.Validator.Engine;

namespace AnalitFramefork.Hibernate.Models
{
	/// <summary>
	/// Модель для формы выполнения / отсылки отчета. Является базовым классом.
	/// </summary>
	public class ReportTemplateForm : FormModel
	{
		public ReportTemplate ReportTemplate { get; set; }
		public ISession DbSession { get; set; }
		/// <summary>
		/// Когда создается отчет изначально, у него еще нет подотчетов и параметров.
		/// Соответственно, чтобы добраться для значений параметров нужно проделать долгий путь.
		/// Чтобы не париться с этим вопросом, мы сначала анализируем законченность шаблона и выставляем данный флаг
		/// </summary>
		[Description("Флаг того, что пользователь завершил все этапы создания шаблона")]
		protected bool TemplateComplete = false;

		//Этот конструктор необходим для маппинга на модель
		public ReportTemplateForm()
		{
			
		}

		public ReportTemplateForm(ReportTemplate template,ISession session)
		{
			ReportTemplate = template;
			if (ReportTemplate.GeneralReport.Reports.Any())
				TemplateComplete = true;
			DbSession = session;
			Initialize();
		}
		/// <summary>
		/// Инициализация класса, которая происходит во время использования конструктора с параметрами
		/// </summary>
		protected virtual void Initialize()
		{
			
		}

		public static ReportTemplateForm CreateReportTemplateForm(ReportTemplate template, ISession session)
		{
			var type = template.Type;
			var typename = Enum.GetName(typeof (ReportTemplateType), type);
			var classname = typename + "ReportTemplateForm";
            var formType = typeof(ReportTemplateForm).Assembly.GetTypes().FirstOrDefault(i => i.Name == classname);
			if (formType == null)
				throw new Exception(string.Format("Не найден класс {0}", classname));

			var instance = Activator.CreateInstance(formType, template, session) as ReportTemplateForm;
			return instance;
		}

		public virtual void Refresh()
		{
			
		}

		public virtual void Save(ISession session)
		{
			
		}
	}
}