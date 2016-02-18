using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	[SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")] // баг решарпера - не может разрешить шаблоны в абстрактных классах
	public abstract class CronParam : TriggerParam
	{
		private string _cronHumanText;

		[Display(Name = "Расписание")]
		[UIHint("CronExpression")]
		[Required(ErrorMessage = "Не задано расписание")]
		public string CronExpressionUi {
			get { return CronExpression.Substring(2).Replace("?", "*"); }
			set { CronExpression = UiToQuartz(value); }
		}

		[ScaffoldColumn(false)]
		public string CronExpression { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string CronHumanText {
			get { return _cronHumanText; }
			set { _cronHumanText = value
					.Replace("Каждый(ую) день", "Каждый день")
					.Replace("Каждый(ую) неделю", "Каждую неделю")
					.Replace("Каждый(ую) месяц", "Каждый месяц")
					.Replace("Каждую неделю по каждый день недели", "Каждый день")
					.Replace("Каждый месяц по каждый день месяца", "Каждый день");
			}
		}

		protected CronParam()
		{
			// значение по умолчанию для UI - без первого нуля для секунд и без знаков "?"
			CronExpressionUi = "0 10 * * 1";
		}

		public override List<ErrorMessage> Validate()
		{
			var errors = new List<ErrorMessage>();
			var arrInput = CronExpressionUi.Split(' ');
			if (arrInput.Length != 5)
				errors.Add(new ErrorMessage("", "Неправильный формат строки Cron"));

			if (MailTo == null)
				errors.Add(new ErrorMessage("MailTo", "Не указан email"));
			else { 
				var ea = new EmailAddressAttribute();
				var ok = true;
				foreach (var mail in MailTo)
					ok = ok && ea.IsValid(mail);
				if (!ok)
					errors.Add(new ErrorMessage("MailTo", "Неверный формат email"));
			}

			return errors;
		}

		public override Dictionary<string, object> ViewDataValues(NamesHelper h)
		{
			var viewDataValues = new Dictionary<string, object>();
			viewDataValues.Add("MailTo", h.GetMailList());
			return viewDataValues;
		}

		private string UiToQuartz(string cronExprUi)
		{
			var arrInput = cronExprUi.Split(' ');

			var arrOutput = new string[6];
			arrOutput[0] = "0";					// секунды
			arrOutput[1] = arrInput[0]; // минуты
			arrOutput[2] = arrInput[1]; // часы
			arrOutput[3] = arrInput[2]; // дни месяца
			arrOutput[4] = arrInput[3]; // месяцы
			arrOutput[5] = "?";					// дни недели

			if (arrInput[4] != "*") {
				arrOutput[3] = "?";
				arrOutput[5] = arrInput[4];
			}

			return string.Join(" ", arrOutput);
		}

	}
}