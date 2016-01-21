using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Quartz.Job.Models
{
	public class Cron
	{

		[Display(Name = "Расписание")]
		[UIHint("CronExpression")]
		[Required(ErrorMessage = "Не задано расписание")]
		public string CronExpression { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string CronHumanText { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string JobName { get; set; }

		[HiddenInput(DisplayValue = false)]
		[Required]
		public string JobGroup { get; set; }
}
}