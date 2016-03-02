using ProducerInterfaceCommon.ContextModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.ViewModel.ControlPanel.Report
{
	public class SearchProducerReportsModel
	{
		[UIHint("Producer")]
		[Display(Name = "Производитель")]
		public long? Producer { get; set; }

		[Display(Name = "Активность")]
		public bool? Enable { get; set; }

		[Display(Name = "Название")]
		public string ReportName { get; set; }

		[Display(Name = "Тип")]
		public int? ReportType { get; set; }

		[Display(Name = "Запуск с...")]
		public DateTime? RunFrom { get; set; }

		[Display(Name = "Запуск по...")]
		public DateTime? RunTo { get; set; }

		[HiddenInput(DisplayValue = false)]
		public int CurrentPageIndex { get; set; }

	}
}
