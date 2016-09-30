using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
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
			session.Flush();
			Open();
			//проверяем что есть надпись, т.е. нет баннеров
			AssertText("Аналит Фармация");
			Assert.That(browser.FindElementsByCssSelector(".center-block img").Count,Is.EqualTo(0));
			//добавляем баннеры (без файлов)
			for (var i = 0; i < 3; i++) {
				session.Save(new Slide {ImagePath = i, Enabled = true, LastEdit = SystemTime.Now()});
			}
			session.Flush();
			Open();
			//проверяем, что появились картинки
			WaitForVisibleCss(".center-block img");
		}
	}
}