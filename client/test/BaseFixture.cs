using System;
using System.Data.Entity;
using System.Linq;
using Common.Tools;
using Common.Tools.Calendar;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using ProducerInterfaceCommon.CatalogModels;
using ProducerInterfaceCommon.ContextModels;
using Test.Support.Selenium;

namespace ProducerInterface.Test
{
	public class BaseFixture : SeleniumFixture
	{
		protected producerinterface_Entities db = new producerinterface_Entities();
		protected Context db2 = new Context();
		protected  catalogsEntities ccntx = new catalogsEntities();

		[SetUp]
		public void Start()
		{
			var permissions = db.AccountPermission.ToList();
			db.AccountGroup.Where(s => s.Name == "Администраторы").ToList()
				.Each(u => {
					permissions.Each(f => {
						if (!u.AccountPermission.ToList().Any(s => s.Id == f.Id)) {
							u.AccountPermission.Add(f);
						}
					});
				});
			db.SaveChanges();
		}

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

		protected void ConfirmDialog(string textToCheck = "")
		{
			WaitForVisibleCss("#messageConfirmDialog");
			if (textToCheck != string.Empty) {
				AssertText(textToCheck);
			}
			Click(By.CssSelector("#messageConfirmationLink"));
		}

		protected void ChoseRegion(string id)
		{
			Eval($"$('{id}').val('1').trigger('chosen:updated').change();");
		}
	}
}