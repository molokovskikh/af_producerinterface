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
using OfficeOpenXml.Drawing;

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


			var pieChart = (ExcelPieChart)ws.Drawings.AddChart("PieChart", eChartType.Pie);
			pieChart.SetPosition(dataStartRow - 1, 0, 5, 0);
			pieChart.SetSize(700, 330);
			pieChart.Fill.Color = Color.White;
			pieChart.Border.LineStyle = eLineStyle.Solid;
			pieChart.Border.Width = 1;
			pieChart.Border.Fill.Color = Color.Black;
			//pieChart.SetPlotAreaWidth(0.49m);

			//Set the data range
			var s = new ExcelAddressBase(dataStartRow + 1, 3, dataStartRow + dataTable.Rows.Count - 1, 3);
			var xs = new ExcelAddressBase(dataStartRow + 1, 1, dataStartRow + dataTable.Rows.Count - 1, 1);
			var series = pieChart.Series.Add(ws.Cells[s.Address], ws.Cells[xs.Address]);
			var pieSeries = (ExcelPieChartSerie)series;
			pieSeries.DataLabel.ShowValue = false;

			//Format the legend
			pieChart.Legend.Add();
			pieChart.Legend.Border.Width = 0;
			pieChart.Legend.Font.SetFromFont(new Font("Calibry", 9));
			pieChart.Legend.Position = eLegendPosition.Right;
			//pieChart.SetLegendWidth(0.49m);

			// установили цвета сегментов, сами не устанавливаются
			for (int i = 0; i < dataTable.Rows.Count; i++)
				pieChart.SetDataPointStyle(dataPointIndex: i, color: GetColor(i));

			// диапазон, занимаемый данными
			return new ExcelAddressBase(dataStartRow, 1, dataStartRow + dataTable.Rows.Count, dataTable.Columns.Count);
		}

		private Color GetColor(int index)
		{
			var colorArray = new string[] { "#004586", "#FF420E", "#FFD320", "#579D1C", "#7E0021", "#83CAFF", "#314004", "#AECF00", "#4B1F6F", "#FF950E", "#C5000B", "#0084D1" };
			// цвета повторяются по кругу
			index = index % colorArray.Length;
			var hex = colorArray[index];
			return ColorTranslator.FromHtml(hex);
		}

	}
}