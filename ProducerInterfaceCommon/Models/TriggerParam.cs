using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class TriggerParam
	{
		[Required]
		[ScaffoldColumn(false)]
		public long UserId { get; set; }

		[Display(Name = "Отправить на email")]
		[UIHint("MailTo")]
		public List<string> MailTo { get; set; }

		public abstract List<ErrorMessage> Validate();

		public abstract Dictionary<string, object> ViewDataValues(NamesHelper h);
	}
}