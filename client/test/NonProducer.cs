using NUnit.Framework;

namespace ProducerInterface.Test
{
	public class NonProducer : BaseFixture
	{
		private string username;

		public NonProducer()
		{
			username = "r.kvasov@analit.net";
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
	}
}