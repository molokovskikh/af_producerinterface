using NUnit.Framework;

namespace ProducerInterface.Test
{
	public class NonProducer : BaseFixture
	{
		private string username;

		public NonProducer()
		{
			username = "133@analit.net";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void Check_profile()
		{
			Open();
			AssertText("Отчеты");
			AssertNoText("Акции");
			Css("#profile-dropdown").Click();
			Click("Мой профиль");
			AssertText("Компания");
			AssertText("Фамилия");
		}

		[Test]
		public void Add_report()
		{
			Open();
			Click("Отчеты");
			AssertText("Создание нового отчета");
			Css("#id").SelectByText("Рейтинг товаров");
			Click("Создать");
			AssertText("Рейтинг товаров");
			Css("#CastomName").SendKeys("Рейтинг товаров");
			ChoseRegion("#RegionCodeEqual");
			Click("Сохранить");
			AssertText("Отчет успешно добавлен");
		}
	}
}