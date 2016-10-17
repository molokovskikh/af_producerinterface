using System;
using System.Configuration;
using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class ReportFixture : BaseFixture
	{
		private string username;

		public ReportFixture()
		{
			username = "r.kvasov@analit.net";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void ReportCreate()
		{
			Open();
			Click("Отчеты");
			AssertText("Создание нового отчета");
			Css("#id").SelectByText("Рейтинг поставщиков");
			Click("Создать",false);
			Css("#CastomName").SendKeys("Рейтинг поставщиков");
			ChoseRegion("#RegionCodeEqual");
			Click("Сохранить");
			AssertText("Отчет успешно добавлен");
		}

		[Test]
		public void ReportDelete()
		{
			//создание отчета
			ReportCreate();
			Open("Report");
			var order = session.Query<Job>().Where(s => s.Enable).OrderByDescending(s => s.CreationDate).First();
			//проверка, есть ли на форме созданный элемент, удаляем его
			var orderToCLick =
				browser.FindElementsByCssSelector("#page_content a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick != null);
			orderToCLick.Click();
			//подтверждение удаления
			ConfirmDialog("Вы уверены что хотите удалить отчет");
			WaitForText("Отчет удален");
			//проверка, есть ли на форме удаленный элемент
			orderToCLick =
				browser.FindElementsByCssSelector("#page_content a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick == null);
		}

		[Test]
		public void ReportDeleteOld()
		{
			var deleteOldReportsTerm = int.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
			//создание двух отчетов
			ReportCreate();
			ReportCreate();
			//один должен быть просроченным
			var order = session.Query<Job>().Where(s => s.Enable).OrderByDescending(s => s.CreationDate).First();
			order.LastRun = SystemTime.Now().AddMonths(-deleteOldReportsTerm);
			session.Save(order);
			Open("Report");
			WaitForText("Найдены отчеты, не запускавшиеся более полугода");
			//проверка, есть ли на форме созданный элемент, его удаление
			var orderToCLick =
				browser.FindElementsByCssSelector("#page_content a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick != null);
			//подтверждение удаления
			Click(By.CssSelector("a[href*='DeleteOld']"));
			AssertText("Отчеты, не запускавшиеся больше года");
			//проверка, есть ли на форме удаленный элемент
			orderToCLick =
				browser.FindElementsByCssSelector("#page_content a.deleteItem")
					.FirstOrDefault(s => s.GetAttribute("href").IndexOf(order.JobName) != -1);
			Assert.IsTrue(orderToCLick == null);
		}
	}
}