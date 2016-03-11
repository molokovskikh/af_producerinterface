using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class PharmacyRatingReportRow : ReportRow
	{
		[Display(Name = "Аптека")]
		public string PharmacyName { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля в %")]
		public decimal? SummPercent { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Сумма")]
		public decimal? Summ { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			var clist = list.Cast<PharmacyRatingReportRow>().ToList();

			// сумма всего
			var sm = clist.Sum(x => x.Summ ?? 0);
			foreach (var item in clist)
			{
				if (!item.Summ.HasValue || sm == 0)
					continue;
				item.SummPercent = item.Summ.GetValueOrDefault() * 100 / sm;
			}

			if (clist.Count > 0)
				clist.Add(new PharmacyRatingReportRow() { PharmacyName = "Итоговая сумма", Summ = sm });

			return clist.Cast<T>().ToList();
		}
	}
}