using System;
using System.Drawing;
using CassiniDev;
using Castle.ActiveRecord;
using NUnit.Framework;
using Test.Support.Selenium;
using TestSupport = Test.Support;

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

			_webServer = SeleniumFixture.StartServer("../../../app/");
			SeleniumFixture.GlobalDriver.Manage().Window.Size = new Size(1920, 1080);
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			TestSupport.Setup.BuildConfiguration("db");
			var holder = ActiveRecordMediator.GetSessionFactoryHolder();
			var cfg = holder.GetConfiguration(typeof (ActiveRecordBase));
			var init = new ProducerInterfaceCommon.ContextModels.NHibernate();
			init.Configuration = cfg;
			init.Init();
			var factory = holder.GetSessionFactory(typeof (ActiveRecordBase));
			TestSupport.IntegrationFixture2.Factory = factory;
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			SeleniumFixture.GlobalTearDown();
			_webServer?.ShutDown();
		}
	}
}