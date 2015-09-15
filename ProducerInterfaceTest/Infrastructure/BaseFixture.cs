using System;
using System.Linq;
using AnalitFramefork;
using AnalitFramefork.Hibernate.Models;
using AnalitFramefork.Tests;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerInterface.Models;
using ProducerInterfaceTest.Infrastructure.Helpers;

namespace ProducerInterfaceTest.Infrastructure
{
	internal class BaseFixture : SeleniumFixture
	{
		protected ISession DbSession;
		protected ProducerUser CurrentUser;

		[SetUp]
		public virtual void IntegrationSetup()
		{
			DbSession = Framework.OpenSession(false);
			var modelCleaner = new ModelCleaner();
			modelCleaner.RemoveModels(DbSession);
			var modelGenerator = new ModelGenerator(DbSession);
		}

		[TearDown]
		public virtual void IntegrationTearDown()
		{
			DbSession.Close();
		}

		public void LoginForUser()
		{
			var currentUser = DbSession.Query<ProducerUser>().FirstOrDefault();
			CurrentUser = currentUser;
			Open("/Registration");
			var userName = currentUser.Email;
			var userPassword = ProducerInterfaceTest.Config.GetParam("DefaultUserPassword");
			Css("#login").SendKeys(userName);
			Css("#password").SendKeys(userPassword);
			Css("#getAuthorization").Click();
		}
	}
}