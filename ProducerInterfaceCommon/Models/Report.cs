using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ProducerInterfaceCommon.Heap;
using Quartz;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class Report
	{
		public abstract string Name { get; }

		[Display(Name = "Название отчета")]
		[Required(ErrorMessage = "Не указано название отчета")]
		[StringLength(250, ErrorMessage = "Название отчета должно быть не длиннее 250 символов")]
		[UIHint("String")]
		public string CastomName { get; set; }

		//[HiddenInput(DisplayValue = false)]
		//[Required]
		//public string MailSubject { get; set; }

		// тип отчета по enum
		[HiddenInput(DisplayValue = false)]
		[Required]
		public int Id { get; set; }

		[ScaffoldColumn(false)]
		[Required]
		public long ProducerId { get; set; }

		public abstract List<string> GetHeaders(HeaderHelper h);

		public abstract string GetSpName();

		public abstract Dictionary<string, object> GetSpParams();

		public virtual List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();
			return errors;
		}

		public abstract Dictionary<string, object> ViewDataValues(NamesHelper h);

		public abstract IProcessor GetProcessor(); 

	}
}