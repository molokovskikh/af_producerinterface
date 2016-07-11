using NUnit.Framework;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class ModelFixture
	{
		[Test]
		public void Load_regions()
		{
			var cntx = new producerinterface_Entities();
			var regions =  cntx.Regions();
			Assert.That(regions.Count, Is.GreaterThan(0));
			Assert.That(regions[0].Id, Is.GreaterThan(0));
		}
	}
}