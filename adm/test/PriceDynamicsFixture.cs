using NUnit.Framework;
using NUnit.Framework.Internal;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using Quartz;

namespace test
{
	[TestFixture]
	public class PriceDynamicsFixture
	{
		[Test]
		public void Run_report_for_non_producer()
		{
			var report = new ProductPriceDynamicsReport {
				RegionCodeEqual = { 1 }
			};
			report.Read<ProductPriceDynamicsReportRow>().ToArray();
		}

		[Test]
		public void Product_ratin()
		{
			var report = new ProductRatingReport {
				RegionCodeEqual = { 1 },
				Var = CatalogVar.AllAssortment
			};
			report.Read<ProductRatingReportRow>().ToArray();
		}
	}
}