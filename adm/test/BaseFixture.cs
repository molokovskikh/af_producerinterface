using System;
using System.Linq;
using Common.Tools.Calendar;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Test.Support.Selenium;

namespace test
{
	[TestFixture]
	public class BaseFixture : SeleniumFixture
	{
		protected void WaitAjax(int seconds)
		{
			new WebDriverWait(browser, seconds.Second())
				.Until(d => Convert.ToInt32(Eval("return $.active")) == 0);
		}

		protected void WaitForText(string text, int seconds)
		{
			var wait = new WebDriverWait(browser, seconds.Second());
			wait.Until(d => ((RemoteWebDriver) d).FindElementByCssSelector("body").Text.Contains(text));
		}

		protected void ClickTheLinkWith(string hrefPart)
		{
			browser.FindElementsByCssSelector($"[href*='{hrefPart}']").First().Click();
		}

		protected string GetRandomMail(int length = 5)
		{
			return Guid.NewGuid().ToString().Replace("-", "").Substring(0, length) + "@randomAnalit.net";
		}

		protected void ClosePreviousTab()
		{
			var tabs2 = browser.WindowHandles.ToList();
			browser.SwitchTo().Window(tabs2[0]);
			browser.Close();
			browser.SwitchTo().Window(tabs2[1]);
		}
		protected dynamic Css(string selector)
		{
			return Css(browser, selector);
		}

		protected void CloseAllTabsButOne()
		{
			var allTabsToClose = GlobalDriver.WindowHandles.ToList();

			if (allTabsToClose.Count > 1)
				for (var i = 1; i < allTabsToClose.Count; i++) {
					if (GlobalDriver.CurrentWindowHandle != allTabsToClose[i]) {
						GlobalDriver.SwitchTo().Window(allTabsToClose[i]);
						GlobalDriver.Close();
					}
				}
		}
	}
}