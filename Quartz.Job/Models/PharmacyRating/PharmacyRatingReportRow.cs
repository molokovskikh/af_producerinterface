using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Quartz.Job.Models
{
	public class PharmacyRatingReportRow : RatingReportRow
	{
		[Display(Name = "Наименование и форма выпуска")]
		public string CatalogName { get; set; }

		[Display(Name = "Аптека")]
		public string PharmacyName { get; set; }

	}
}