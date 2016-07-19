using System;
using CassiniDev;
using NUnit.Framework;
using Test.Support;
using Test.Support.Selenium;

namespace test
{
	[SetUpFixture]
	public class Setup
	{
		private Server server;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			SeleniumFixture.GlobalSetup();
			server = SeleniumFixture.StartServer("../../../app/");
			IntegrationFixture2.Factory = Test.Support.Setup.Initialize("db");
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			SeleniumFixture.GlobalTearDown();
			server?.Dispose();
		}
	}
}