using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.Heap
{
	static class Class1
	{
		public static void SetDataPointStyle(this ExcelPieChart chart, int dataPointIndex, Color color)
		{
			//Get the nodes
			var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
			var nschart = nsm.LookupNamespace("c");
			var nsa = nsm.LookupNamespace("a");
			var node = chart.ChartXml.SelectSingleNode("c:chartSpace/c:chart/c:plotArea/c:pieChart/c:ser", nsm);
			var doc = chart.ChartXml;
			//Add the node
			//Create the data point node
			var dPt = doc.CreateElement("c:dPt", nschart);

			var idx = dPt.AppendChild(doc.CreateElement("c:idx", nschart));
			var valattrib = idx.Attributes.Append(doc.CreateAttribute("val"));
			valattrib.Value = dataPointIndex.ToString(CultureInfo.InvariantCulture);
			node.AppendChild(dPt);

			//Add the solid fill node
			var spPr = doc.CreateElement("c:spPr", nschart);
			var solidFill = spPr.AppendChild(doc.CreateElement("a:solidFill", nsa));
			var srgbClr = solidFill.AppendChild(doc.CreateElement("a:srgbClr", nsa));
			valattrib = srgbClr.Attributes.Append(doc.CreateAttribute("val"));

			//Set the color
			valattrib.Value = color.ToHex().Substring(1);
			dPt.AppendChild(spPr);
		}

		public static String ToHex(this Color c)
		{
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}
	}
}
