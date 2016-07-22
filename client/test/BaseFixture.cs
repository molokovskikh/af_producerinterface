using ProducerInterfaceCommon.ContextModels;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	public class BaseFixture : SeleniumFixture
	{
		protected producerinterface_Entities db = new producerinterface_Entities();
		protected Context db2 = new Context();

		protected void ChoseRegion(string id)
		{
			Eval($"$('{id}').val('1').trigger('chosen:updated').change();");
		}
	}
}
