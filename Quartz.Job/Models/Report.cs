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

		[ScaffoldColumn(false)]
		[Required]
		public long ProducerId { get; set; }

		[Display(Name = "Отправить на e-mail")]
		[Required(ErrorMessage = "Не указаны e-mail")]
		[UIHint("MailTo")]
		public List<string> MailTo { get; set; }

		public abstract List<string> GetHeaders(HeaderHelper h);

		public abstract Report Process(JobKey key, Report jparam, TriggerParam tparam);

		public abstract string GetSpName();

		public abstract Dictionary<string, object> GetSpParams();

		public virtual List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();
			if (MailTo == null)
				return errors;
			var ea = new EmailAddressAttribute();
			var ok = true;
			foreach (var mail in MailTo)
				ok = ok && ea.IsValid(mail);
			if(!ok)
				errors.Add(new ErrorMessage("MailTo", "Неверный формат почтового адреса"));
			return errors;
		}

		public abstract Dictionary<string, object> ViewDataValues(NamesHelper h);

	}
}