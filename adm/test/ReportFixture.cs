using System;
using System.Configuration;
using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.Models;

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
	}
}