using System;
using System.Security.Policy;
using NUnit.Framework;
using Test.Support.Selenium;

namespace test
{
	[TestFixture]
	public class MenuFixture : SeleniumFixture
	{
		private string username;

		public MenuFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void View_promotions()
		{
			Open();
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Акции");
			AssertText("Промо-Акции");
		}

		[Test]
		public void Show_reports()
		{
			Open();
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Отчеты");
			AssertText("Отчеты");
			AssertText(username);
			AssertText("Разделы сайта");
		}
	}
}