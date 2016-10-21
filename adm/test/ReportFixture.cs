using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.TasksManager.Services;

namespace test
{
	[TestFixture]
	public class ReportFixture : BaseFixture
	{
		private string username;

		public ReportFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void ReportDelete()
		{
			Open();
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Отчеты");
			var order = session.Query<Job>().Where(s => s.Enable).OrderByDescending(s => s.CreationDate).First();
			//проверка, есть ли на форме созданный элемент, удаляем его
			var orderToCLick =
				browser.FindElementsByCssSelector("table a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick != null);
			orderToCLick.Click();
			//подтверждение удаления
			ConfirmDialog("Вы уверены что хотите удалить отчет");
			WaitForText("Отчет удален");
			//проверка, есть ли на форме удаленный элемент
			orderToCLick =
				browser.FindElementsByCssSelector("table a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick == null);
		}

		[Test]
		public void EmailNotifierOldReportsFixture()
		{
			var list = session.Query<ServiceTaskManager>().ToList();
			list.ForEach(s => { session.Delete(s); });
			Open();
			AssertText("Статистика");

			Open("/adminaccount/ServiceJobList");
			AssertText(typeof(EmailNotifierOldReports).Name);
			var item = session.Query<ServiceTaskManager>().FirstOrDefault(s=>s.ServiceType == typeof(EmailNotifierOldReports).FullName);

			Assert.IsFalse(item.Enabled);

			Click(By.CssSelector("#interval" + item.Id));
			WaitForText($"Задача {item.Id} запущена успешно");
			Open("/adminaccount/ServiceJobList");
			session.Refresh(item);
			Assert.IsTrue(item.Enabled);
			Assert.IsFalse(item.LastRun.HasValue);

			Thread.Sleep(65000);
			Open("/adminaccount/ServiceJobList");
			session.Refresh(item);
			Assert.IsTrue(item.Enabled);
			Assert.IsTrue(item.LastRun.HasValue);
		}
	}
}