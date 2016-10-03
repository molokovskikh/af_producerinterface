using System;
using NUnit.Framework;

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
		public void Create_report()
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
	}
}