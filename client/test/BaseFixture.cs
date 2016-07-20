using ProducerInterfaceCommon.ContextModels;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	public class BaseFixture : SeleniumFixture
	{
		protected producerinterface_Entities db = new producerinterface_Entities();
		protected Context db2 = new Context();
	}
}
