﻿using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class RunNowParam : TriggerParam
	{

		[ScaffoldColumn(false)]
		public string Ip { get; set; }

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();
			viewDataValues.Add("MailTo", h.GetMailList());
			return viewDataValues;
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();

			if (MailTo != null && MailTo.Count > 0) {
				var ea = new EmailAddressAttribute();
				var ok = true;
				foreach (var mail in MailTo)
					ok = ok && ea.IsValid(mail);
				if (!ok)
					errors.Add(new ErrorMessage("MailTo", "Неверный формат email"));
			}
			return errors;
		}
	}
}