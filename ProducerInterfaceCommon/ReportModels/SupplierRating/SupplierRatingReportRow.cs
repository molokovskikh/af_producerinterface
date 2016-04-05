using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ProducerInterfaceCommon.Models
{
	public class SupplierRatingReportRow : ReportRow, IWriteExcelData
	{
		[Display(Name = "Поставщик")]
		public string SupplierName { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Доля в % (рубли)")]
		public decimal? SummPercent { get; set; }

		[Format(Value = "0.00")]
		[Round(Precision = 2)]
		[Display(Name = "Сумма, руб.")]
		public decimal? Summ { get; set; }

		public override List<T> Treatment<T>(List<T> list, Report param)
		{
			var clist = list.Cast<SupplierRatingReportRow>().ToList();
			// уже отсортированно по Summ desc
			// оставили только 16 строк лучших 
			if (clist.Count() > 16) {
				var top = clist.Take(15).ToList();
				var skipSum = clist.Skip(15).Sum(x => x.Summ ?? 0);
				top.Add(new SupplierRatingReportRow() { SupplierName = "Остальные", Summ = skipSum });
				clist = top;
			}

			// сумма всего
			var sm = clist.Sum(x => x.Summ ?? 0);
			foreach (var item in clist)
			{
				if (!item.Summ.HasValue || sm == 0)
					continue;
				item.SummPercent = item.Summ.GetValueOrDefault() * 100 / sm;
			}

			clist.Add(new SupplierRatingReportRow() { SupplierName = "Итоговая сумма", Summ = sm } );

			return clist.Cast<T>().ToList();
		}

		public ExcelAddressBase WriteExcelData(ExcelWorksheet ws, int dataStartRow, DataTable dataTable, Report param)
		{
			// установили форматы для открытых колонок, скрытые удалили
			var type = typeof(SupplierRatingReportRow);

			int j = 1;
			foreach (var p in type.GetProperties())
			{
				if (!Attribute.IsDefined(p, typeof(HiddenAttribute)))
				{
					var f = p.GetCustomAttribute<FormatAttribute>();
					if (f != null)
						ws.Column(j).Style.Numberformat.Format = f.Value;
					j++;
				}
				else
				{
					dataTable.Columns.Remove(p.Name);
				}
			}
			ws.Cells[dataStartRow, 1].LoadFromDataTable(dataTable, true);

			//var xs = new ExcelAddressBase(dataStartRow + 1, 1, dataStartRow + dataTable.Rows.Count - 1, 1);
			//var s = new ExcelAddressBase(dataStartRow + 1, 3, dataStartRow + dataTable.Rows.Count - 1, 3);

			//var chart = ws.Drawings.AddChart("chart", eChartType.Pie);
			//chart.SetPosition(dataStartRow, 20, 5, 10);
			////chart.SetSize((800, 600);
			//var series = chart.Series.Add(ws.Cells["C6:C16"], ws.Cells["A6:A16"]);

			//var pieSeries = (ExcelPieChartSerie)series;
			//pieSeries.Explosion = 5;

			////Format the labels
			//pieSeries.DataLabel.Font.Size = 10;

			//Add the chart to the sheet
			var pieChart = (ExcelPieChart)ws.Drawings.AddChart("Chart1", eChartType.Pie);
			pieChart.SetPosition(dataStartRow, 0, 0, 0);
			pieChart.Title.Text = "Test Chart";
			pieChart.Title.Font.Bold = true;
			pieChart.Title.Font.Size = 10;

			//Set the data range
			var series = pieChart.Series.Add(ws.Cells[5, 3, 16, 3], ws.Cells[5, 1, 16, 1]);
			var pieSeries = (ExcelPieChartSerie)series;

			pieSeries.Explosion = 5;
			pieSeries.DataLabel.Fill.Color = Color.BlueViolet;

			//Format the labels
			pieSeries.DataLabel.Font.Bold = true;
			pieSeries.DataLabel.ShowValue = true;
			pieSeries.DataLabel.ShowPercent = true;
			pieSeries.DataLabel.ShowLeaderLines = true;
			pieSeries.DataLabel.Separator = ";";
			pieSeries.DataLabel.Position = eLabelPosition.BestFit;

			//Format the legend
			pieChart.Legend.Add();
			pieChart.Legend.Border.Width = 0;
			pieChart.Legend.Font.Size = 10;
			pieChart.Legend.Font.Bold = true;
			pieChart.Legend.Position = eLegendPosition.Right;

			pieChart.SetDataPointStyle(dataPointIndex: 2, color: Color.FromArgb(254, 0, 0));


			//ExcelChart chart = ws.Drawings.AddChart("FindingsChart", OfficeOpenXml.Drawing.Chart.eChartType.ColumnClustered);
			//chart.Title.Text = "Category Chart";
			//chart.SetPosition(1, 0, 5, 0);
			//chart.SetSize(800, 300);
			//var ser1 = (ExcelChartSerie)(chart.Series.Add(ws.Cells["C6:C16"], ws.Cells["A6:A16"]));
			//ser1.Header = "Category";
			//Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B7DEE8");
			//chart.Fill.Color = colFromHex;

			// диапазон, занимаемый данными
			return new ExcelAddressBase(dataStartRow, 1, dataStartRow + dataTable.Rows.Count, dataTable.Columns.Count);
		}



	}
}