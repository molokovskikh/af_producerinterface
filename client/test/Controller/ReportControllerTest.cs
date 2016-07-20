using System.Linq;
using NUnit.Framework;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterface.Test.Controller
{
	public class ReportControllerTest : BaseFixture
	{
		private Account account;

		[SetUp]
		public void RcSetUp()
		{
			account = db.Account.Single(x => x.Login == "g.maksimenko@analit.net");
			account.AccountCompany.ProducerId = null;
			db.SaveChanges();

			Open("Account/Auth");
			WaitForVisibleCss(".login2");
			Css(".login2").SendKeys("g.maksimenko@analit.net");
			Css(".password2").SendKeys("123456");
			Css(".enter2").Click();
		}

		[TearDown]
		public void RcTearDown()
		{
			account.AccountCompany.ProducerId = 5;
			db.SaveChanges();

			Open("Account/LogOut");
			//CloseAllTabsButOne();
		}

		[Test, Description("Доступ в раздел Отчеты закрыт для пользователей без производителя")]
		public void JobList_UserWithoutProducer_RedirectToHome()
		{
			Open("Report/JobList");
			var result = browser.PageSource.Contains("Доступ в раздел Отчеты закрыт");
			Assert.IsTrue(result);
		}
	}
}
