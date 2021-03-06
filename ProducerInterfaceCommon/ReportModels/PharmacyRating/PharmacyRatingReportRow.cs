﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class PharmacyRatingReportRow : ReportRow
	{
		[Display(Name = "Аптека")]
		public string PharmacyName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля в % (рубли)")]
		public decimal? SummPercent { get; set; }

		[Hidden]
		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Сумма, руб.")]
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

			return clist.Cast<T>().ToList();
		}
	}
}