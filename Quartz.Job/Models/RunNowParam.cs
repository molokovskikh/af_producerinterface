using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Quartz.Job.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class RunNowParam : TriggerParam
	{
		[Display(Name = "Отправить по E-mail")]
		[UIHint("Bool")]
		public bool ByEmail { get; set; }

		[Display(Name = "Отобразить на экране")]
		[UIHint("Bool")]
		public bool ByDisplay { get; set; }

		protected RunNowParam()
		{
			ByEmail = true;
			ByDisplay = true;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();
			//if (!ByEmail && !ByDisplay) {
			//	errors.Add(new ErrorMessage("ByEmail", "Выберите хотя бы один из двух вариантов"));
			//	errors.Add(new ErrorMessage("ByDisplay", "Выберите хотя бы один из двух вариантов"));
			//}
			return errors;
		}
	}
}