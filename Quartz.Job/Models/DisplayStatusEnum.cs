using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
{
		public enum DisplayStatus
		{

			[Display(Name = "Не запускался")]
			New = 0,

			[Display(Name = "Отчёт готовится")]
			Processed = 1,

			[Display(Name = "Отчёт готов")]
			Ready = 2,

			[Display(Name = "Нет данных для построения отчёта")]
			Empty = 3,

			[Display(Name = "В процессе подготовки произошла ошибка")]
			Error = 4
		}
}
