using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using AnalitFramefork.Helpers;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace AnalitFramefork.Tests
{
	/// <summary>
	/// Главная фикстура, от которой все наследуются
	/// </summary>
	public class SeleniumFixture
	{
		/// <summary>
		/// Количество секунд, которые ждет браузер во время использования функции Wait для тестирования Ajax
		/// </summary>
		private static readonly TimeSpan WaitTime = new TimeSpan(0, 0, 0, 5);
		public static string DefaultUrl = "/";
		public RemoteWebDriver Browser;
		public TestSetup TestSetup;
		public class ChromeOptionsWithPrefs : ChromeOptions
		{
			public Dictionary<string, object> prefs { get; set; }
		}

		public static string GetUri(string uri)
		{
			Uri uriObj;
			Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out uriObj);
			if (uriObj != null && !uriObj.IsAbsoluteUri)
				uri = BuildTestUrl(uri);
			return uri;
		}

		public static string BuildTestUrl(string urlPart)
		{
			var WebRoot = Config.GetParam("webRoot","/");
			var WebPort = Config.GetParam("webPort");
			if (!urlPart.StartsWith("/"))
				urlPart = "/" + urlPart;
			if (WebRoot.EndsWith("/") && urlPart.Length > 0)
				urlPart = urlPart.Remove(0, 1);
			return String.Format("http://localhost:{0}{1}{2}",
				WebPort,
				WebRoot,
				urlPart);
		}

		
		protected RemoteWebDriver Open(string uri, params object[] args)
		{
			return Open(String.Format(uri, args));
		}

		/// <summary>
		/// Открытие страницы в браузере.
		/// </summary>
		/// <param name="url">Относительный адрес страницы, например home/plans</param>
		/// <returns></returns>
		public RemoteWebDriver Open(string url = null)
		{
			url = url ?? DefaultUrl;

			url = GetUri(url);

			if (Browser == null) {
				Browser = TestSetup.Browser;
			}

			Browser.Navigate().GoToUrl(url);

			return Browser;
		}

		/// <summary>
		/// Ого, тут судя по всему 
		/// </summary>
		[TearDown]
		public void SeleniumTearDown()
		{
			var currentContext = TestContext.CurrentContext;
			if (currentContext.Result.Status == TestStatus.Failed) {
				if (Browser != null) {
					Console.WriteLine(Browser.Url);
					if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG_SELENIUM"))) {
						var data = Browser.Url + Environment.NewLine + Html;
						File.WriteAllText(currentContext.Test.FullName + ".html", data);
						((ITakesScreenshot)Browser).GetScreenshot().SaveAsFile(currentContext.Test.FullName + ".png", ImageFormat.Png);
					}
				}
			}
			Browser = null;
		}

		protected string Html
		{
			get { return Browser.FindElementByTagName("body").GetAttribute("innerHTML"); }
		}

		protected void RunJavaScript(string script)
		{
			(Browser as IJavaScriptExecutor).ExecuteScript(script);
		}

		protected void WaitForElementByLocator(By locator, int iterations = 100)
		{
			for (int i = 0; i < iterations; i++) {
				if (Browser.FindElements(locator).Count > 0)
					break;
				Thread.Sleep(10);
			}
		}

		protected void WaitForCss(string css)
		{
			var wait = new WebDriverWait(Browser, WaitTime);
			wait.Until(d => ((RemoteWebDriver)d).FindElementsByCssSelector(css).Count > 0);
		}

		protected void WaitForVisibleCss(string css)
		{
			var wait = new WebDriverWait(Browser, WaitTime);
			wait.Until(d => ((RemoteWebDriver)d).FindElementByCssSelector(css).Displayed);
		}

		protected void WaitAnimation(string css)
		{
			var wait = new WebDriverWait(Browser, WaitTime);
			wait.Until(d => {
				var javaScriptExecutor = (IJavaScriptExecutor)d;
				var isAnimated = bool.Parse(javaScriptExecutor
					.ExecuteScript(string.Format("return $('{0}').is(':animated')", css))
					.ToString().ToLower());
				return !isAnimated;
			});
		}

		protected void WaitClickable(string css)
		{
			var wait = new WebDriverWait(Browser, WaitTime);
			wait.Until(d => {
				var el = ((RemoteWebDriver)d).FindElementByCssSelector(css);
				return el.Displayed
				       && el.Enabled
					//проверяем что анимация завершилась
				       && el.Location.X > 0
				       && el.Location.Y > 0;
			});
		}

		protected void WaitForText(string text)
		{
			var wait = new WebDriverWait(Browser, WaitTime);
			wait.Until(d => ((RemoteWebDriver)d).FindElementByCssSelector("body").Text.Contains(text));
		}

		//иногда WaitForText приводит к ошибкам stale reference exception
		public void SafeWaitText(string text)
		{
			var begin = DateTime.Now;
			var timeout = WaitTime;
			while (true) {
				try {
					var found = Browser.FindElementByCssSelector("body").Text.Contains(text);
					if (found)
						break;
					if ((DateTime.Now - begin) > timeout)
						throw new Exception(String.Format("Не удалось дождаться текста '{0}'", text));
				}
				catch (StaleElementReferenceException) {
				}
				Thread.Sleep(50);
			}
		}

		protected dynamic FindByText(string text)
		{
			return Browser.FindElementByXPath(String.Format("//*[contains(text(), {0})]", text));
		}

		protected void AssertText(string text)
		{
			var body = Browser.FindElementByCssSelector("body").Text;
			Assert.That(body, Is.StringContaining(text));
		}

		protected void AssertNoText(string text)
		{
			var body = Browser.FindElementByCssSelector("body").Text;
			Assert.That(body, Is.Not.StringContaining(text));
		}

		protected bool IsPresent(string selector)
		{
			return Browser.FindElements(By.CssSelector(selector)).Count > 0;
		}

		protected dynamic Css(string selector)
		{
			return Css(Browser, selector);
		}

		protected dynamic Css(dynamic parent, string selector)
		{
			var element = ((IFindsByCssSelector)parent).FindElementByCssSelector(selector);
			if (element.TagName == "select")
				return new SelectElement(element);
			return element;
		}

		protected void Click(string text)
		{
			var buttons = Browser.FindElementsByCssSelector("a, input[type=button], input[type=submit], button");

			var button = buttons.FirstOrDefault(b => String.Equals(b.GetAttribute("value"), text, StringComparison.CurrentCultureIgnoreCase)) ??
			             buttons.FirstOrDefault(b => String.Equals(b.Text, text, StringComparison.CurrentCultureIgnoreCase));

			if (button == null)
				throw new Exception(String.Format("Элемент с текстом '{0}' не найден!", text));

			button.Click();
		}

		protected void Click(IWebElement el, string text)
		{
			var buttons = el.FindElements(By.CssSelector("a, input[type=button], input[type=submit], button"));

			var button = buttons.FirstOrDefault(b => String.Equals(b.GetAttribute("value"), text, StringComparison.CurrentCultureIgnoreCase)) ??
			             buttons.FirstOrDefault(b => String.Equals(b.Text, text, StringComparison.CurrentCultureIgnoreCase));

			if (button == null)
				throw new Exception(String.Format("Элемент с текстом '{0}' не найден!", text));

			button.Click();
		}


		protected void Click(By locator)
		{
			var element = Browser.FindElement(locator);

			if (element == null)
				throw new Exception("Элемент не найден!");

			element.Click();
		}

		protected void ClickButton(string selector, string value)
		{
			var root = Browser.FindElementByCssSelector(selector);
			var element = root.FindElement(By.CssSelector(String.Format("[value=\"{0}\"]", value)));
			Browser.ExecuteScript(String.Format("window.scrollTo({0},{1})", element.Location.X, element.Location.Y));
			element.Click();
		}

		protected void ClickLink(string selector, string text)
		{
			var root = Browser.FindElementByCssSelector(selector);
			root.FindElement(By.PartialLinkText(text)).Click();
		}

		protected void ClickButton(string value)
		{
			var element = Browser.FindElement(By.CssSelector(String.Format("[value=\"{0}\"]", value)));
			Browser.ExecuteScript(String.Format("window.scrollTo({0},{1})", element.Location.X, element.Location.Y));
			element.Click();
		}

		protected void ClickLink(string text)
		{
			Browser.FindElementsByLinkText(text).First().Click();
		}

		protected object Eval(string js)
		{
			return ((IJavaScriptExecutor)Browser).ExecuteScript(js);
		}

	

		public void WaitAjax()
		{
			new WebDriverWait(Browser, WaitTime)
				.Until(d => Convert.ToInt32(Eval("return $.active")) == 0);
		}
	}
}