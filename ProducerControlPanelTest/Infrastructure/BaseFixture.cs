using System;
using System.Linq;
using AnalitFramefork;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Tests;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerControlPanelTest.Infrastructure.Helpers;

namespace ProducerControlPanelTest.Infrastructure
{
	internal class BaseFixture : SeleniumFixture
	{
		protected ISession DbSession;
		protected Admin CurrentAdmin;

		[SetUp]
		public virtual void IntegrationSetup()
		{
			DbSession = Framework.OpenSession(false);
			DbSession.FlushMode= FlushMode.Always;
			var modelCleaner = new ModelCleaner();
			modelCleaner.RemoveModels(DbSession);
			var modeleGenerator = new ModelGenerator(DbSession);
		}

		[TearDown]
		public virtual void IntegrationTearDown()
		{
			DbSession.Close();
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