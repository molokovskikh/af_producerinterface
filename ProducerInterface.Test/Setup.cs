using System;
using CassiniDev;
using NUnit.Framework;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			SeleniumFixture.GlobalSetup();
			_webServer = SeleniumFixture.StartServer("../../../ProducerInterface/");
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			SeleniumFixture.GlobalTearDown();
			_webServer.ShutDown();
		}
	}
}