using System;
using System.Linq;
using AnalitFramefork;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Tests;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerControlPanel;
using ProducerInterface;
using ProducerControlPanelTest.Infrastructure.Helpers;

namespace ProducerControlPanelTest.Infrastructure
{
	internal class ProducerInterfaceControlPanelFixture : SeleniumFixture
	{
		protected ISession DbSession;
		protected Admin CurrentAdmin;

		[SetUp]
		public virtual void IntegrationSetup()
		{
			// !эти две строчки нужны для создания ссылок на проекты, при маппинге моделей |==>>>
			new ProducerControlPanel.Config();
			new ProducerInterface.Config();
			//<<<==|
			DbSession = Framework.OpenSession(false);
			DbSession.FlushMode= FlushMode.Always;
			var modelCleaner = new ModelCleaner();
			modelCleaner.RemoveModels(DbSession);
			var modeleGenerator = new ModelGenerator(DbSession);

			LoginForAdmin();
		}

		[TearDown]
		public virtual void IntegrationTearDown()
		{
			DbSession.Close();
			Css(".entypo-logout.right").Click();
		}

		public void LoginForAdmin()
		{
			Open("/");
			var adminName = Environment.UserName;
			var adminPassword = ProducerControlPanelTest.Config.GetParam("DefaultUserPassword");
			Css("#username").SendKeys(adminName);
			Css("#password").SendKeys(adminPassword);
			Css(".btn-login").Click();
			AssertText(adminName);
			var currentAdmin = DbSession.Query<Admin>().First(i => i.UserName == adminName);
			CurrentAdmin = currentAdmin;
		}
	}
}