using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.Models
{
		public enum Reports
		{
			[Display(Name = "Рейтинг товаров")]
			ProductRatingReport = 1,

			[Display(Name = "Рейтинг аптек")]
			PharmacyRatingReport = 2,

			[Display(Name = "Рейтинг поставщиков")]
			SupplierRatingReport = 3
		}
}
