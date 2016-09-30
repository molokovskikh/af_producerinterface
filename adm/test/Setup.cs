using System;
using CassiniDev;
using Castle.ActiveRecord;
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

			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			Test.Support.Setup.BuildConfiguration("db");
			var holder = ActiveRecordMediator.GetSessionFactoryHolder();
			var cfg = holder.GetConfiguration(typeof (ActiveRecordBase));
			var init = new ProducerInterfaceCommon.ContextModels.NHibernate();
			init.Configuration = cfg;
			init.Init();
			var factory = holder.GetSessionFactory(typeof (ActiveRecordBase));
			IntegrationFixture2.Factory = factory;
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			SeleniumFixture.GlobalTearDown();
			server?.Dispose();
		}
	}
}