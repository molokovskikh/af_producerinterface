using ProducerInterfaceCommon.ContextModels;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	public class BaseFixture : SeleniumFixture
	{
		protected producerinterface_Entities cntx = new producerinterface_Entities();
	}
}
