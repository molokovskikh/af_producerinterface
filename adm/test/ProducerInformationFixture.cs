using System;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerInterfaceCommon.Models;

namespace test
{
	public class ProducerInformationFixture : BaseFixture
	{
		private readonly string username;

		public ProducerInformationFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void ReportScheduleJob_p85()
		{
			Open();
			var job = session.Query<Job>().OrderByDescending(s => s.Id).FirstOrDefault(s => s.JobGroup == "p85");
			Open($"/Report/ScheduleJob?jobName={job.JobName}&jobGroup={job.JobGroup}");
			WaitForText("Расписание отчета", 20);
			AssertText("Рейтинг поставщиков");
			var inputObj = browser.FindElementByCssSelector("#MailTo_addMail");
			inputObj.Clear();
			var newMail = GetRandomMail();
			inputObj.SendKeys(newMail);
			inputObj = browser.FindElementByCssSelector("#addBtn");
			inputObj.Click();
			Click("Применить");
			AssertText("Время формирования отчета успешно установлено");
			Open($"/Report/ScheduleJob?jobName={job.JobName}&jobGroup={job.JobGroup}");
			WaitForText("Расписание отчета", 10);
			Assert.IsTrue(browser.FindElementsByCssSelector($"#MailTo option[value='{newMail}']").ToList().Count > 0);
		}

		[Test]
		public void ReportTableData()
		{
			Open();
			Open("ProducerInformation");
			WaitForText("Выберите производителя");
			Css("#CompanySelectId").SelectByText("BELUPO D.D.");
			WaitAjax(15);
			ClickLink("Отчеты");
			WaitForText("Расписание");
			ClickTheLinkWith("/Report/ScheduleJob");
			ClosePreviousTab();
			WaitForText("Расписание отчета", 20);
		}
	}
}