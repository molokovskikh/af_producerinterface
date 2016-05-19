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
			account = cntx.Account.Single(x => x.Login == "g.maksimenko@analit.net");
			account.AccountCompany.ProducerId = null;
			cntx.SaveChanges();

			Open("Account/Auth");
			WaitForVisibleCss(".login2", 10);
			Css(".login2").SendKeys("g.maksimenko@analit.net");
			Css(".password2").SendKeys("d45a43");
			Css(".enter2").Click();
		}

		[TearDown]
		public void RcTearDown()
		{
			account.AccountCompany.ProducerId = 5;
			cntx.SaveChanges();

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
