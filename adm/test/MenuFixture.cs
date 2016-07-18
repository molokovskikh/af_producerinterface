using System;
using System.Security.Policy;
using NUnit.Framework;
using Test.Support.Selenium;

namespace test
{
	[TestFixture]
	public class MenuFixture : SeleniumFixture
	{
		[Test]
		public void View_promotions()
		{
			Open("/?debug-user=kvasov");
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Акции");
			AssertText("Промо-Акции");
		}
	}
}