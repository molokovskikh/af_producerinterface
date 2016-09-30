using System;
using System.Linq;
using Common.Tools.Calendar;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using ProducerInterfaceCommon.ContextModels;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	public class BaseFixture : SeleniumFixture
	{
		protected producerinterface_Entities db = new producerinterface_Entities();
		protected Context db2 = new Context();

		public void ScrollTo(IWebElement element, int xAdditive = 0, int yAdditive = 0)
		{
			browser.ExecuteScript($"window.scrollTo({element.Location.X + xAdditive},{element.Location.Y + yAdditive})");
		}

		protected void WaitForText(string text, int seconds)
		{
			var wait = new WebDriverWait(browser, seconds.Second());
			wait.Until(d => ((RemoteWebDriver) d).FindElementByCssSelector("body").Text.Contains(text));
		}

		protected void Click(string text, bool doScroll)
		{
			var buttons = browser.FindElementsByCssSelector("a, input[type=button], input[type=submit], button");

			var button =
				buttons.FirstOrDefault(b => string.Equals(b.GetAttribute("value"), text, StringComparison.CurrentCultureIgnoreCase)) ??
					buttons.FirstOrDefault(b => string.Equals(b.Text?.Trim(), text, StringComparison.CurrentCultureIgnoreCase));

			if (button == null)
				throw new Exception($"Элемент с текстом '{text}' не найден!");
			if (doScroll) {
				ScrollTo(button);
			}
			button.Click();
		}


		protected void ChoseRegion(string id)
		{
			Eval($"$('{id}').val('1').trigger('chosen:updated').change();");
		}
	}
}