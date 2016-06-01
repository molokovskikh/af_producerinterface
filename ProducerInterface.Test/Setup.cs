using CassiniDev;
using NUnit.Framework;

namespace ProducerInterface.Test
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			SeleniumFixture.GlobalSetup();
			_webServer = SeleniumFixture.StartServer();
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			SeleniumFixture.GlobalTearDown();
			_webServer.ShutDown();
		}
	}
}