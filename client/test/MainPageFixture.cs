using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Test
{
	public class MainPageFixture : BaseFixture
	{
		[Test]
		public void SliderCheck()
		{
			//чистим список
			var list = session.Query<Slide>().ToList();
			list.ForEach(s => { session.Delete(s); });
			Open();
			if (browser.FindElementsByCssSelector("[role='navigation']").Count > 0) {
				browser.Manage().Cookies.DeleteAllCookies();
				Open();
			}
			//проверяем что есть надпись, т.е. нет баннеров
			AssertText("Аналит Фармация");
			Assert.That(browser.FindElementsByCssSelector(".center-block img").Count,Is.EqualTo(0));
			//добавляем баннеры (без файлов)
			for (var i = 0; i < 3; i++) {
				session.Save(new Slide {ImagePath = i, Enabled = true, LastEdit = SystemTime.Now()});
			}
			Open();
			WaitForText("Аналит Фармация", 20);
			//проверяем, что появились картинки
			WaitForVisibleCss(".center-block img");
		}
	}
}