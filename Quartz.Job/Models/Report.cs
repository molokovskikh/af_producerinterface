using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Quartz.Job.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class Report
	{
		public abstract string Name { get; }

		[Display(Name = "Название отчёта")]
		[Required(ErrorMessage = "Не указано название отчёта")]
		[StringLength(250, ErrorMessage = "Название отчёта должно быть не длиннее 250 символов")]
		[UIHint("String")]
		public string CastomName { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string MailSubject { get; set; }

		// тип отчёта по enum
		[HiddenInput(DisplayValue = false)]
		[Required]
		public int Id { get; set; }

		[Display(Name = "Отправить на e-mail")]
		[Required(ErrorMessage = "Не указаны e-mail")]
		[UIHint("StringList")]
		public List<string> MailTo { get; set; }

		public abstract List<string> GetHeaders(HeaderHelper h);

		public abstract Report Process(Report param, JobKey key);

		public abstract string GetSpName();

		public abstract Dictionary<string, object> GetSpParams();

		public abstract List<ErrorMessage> Validate();

		public abstract Dictionary<string, object> ViewDataValues(NamesHelper h);

	}
}