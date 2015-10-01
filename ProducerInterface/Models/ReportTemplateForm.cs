using System;
using System.Collections.Generic;
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
	/// Модель для формы выполнения / отсылки отчета
	/// </summary>
	public class ReportTemplateForm : FormModel
	{
		public ReportTemplate ReportTemplate { get; set; }
		public ReportTemplateForm()
		{
			
		}
		public ReportTemplateForm(ReportTemplate template,ISession session)
		{
			ReportTemplate = template;
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