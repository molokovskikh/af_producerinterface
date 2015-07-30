using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AnalitFramefork.Helpers;
using IISExpressBootstrapper;
using NHibernate.Util;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace AnalitFramefork.Tests
{
	/// <summary>
	/// Загрузчик тестов. Поднимает сервер, запускает хром и так далее.
	/// Необходимо не забыть добавить аттрибут[SetUpFixture] наследнику данного класса
	/// Также нужно убедиться, что проект с тестами имеет фреймфорк 4.5
	/// </summary>
	[SetUpFixture]
	public class TestSetup
	{
		public static RemoteWebDriver Browser;
		protected Dictionary<int,IISExpressHost> WebServers;

		/// <summary>
		/// Метод, который выполняется перед запуском тестов. Выполняется один раз.
		/// Тут должен находиться код, который запускает браузер, сервер и т.д.
		/// </summary>
		[SetUp]
		public void Setup()
		{

			//Все опасные функции, должны быть вызванны до этого момента, так как исключения в сетапе
			//оставляют невысвобожденные ресурсы браузера и веб сервера
			StartBrowser();
			StartServer();
		}

		/// <summary>
		/// Метод, который выполняется после выполнения всех тестов.
		/// Как правило тут находятся код, который отпускает ресурсы: выключает сервер, выключает браузре и т.д.
		/// </summary>
		[TearDown]
		public void Teardown()
		{
			StopBrowser();
			StopServer();
		}

		/// <summary>
		/// Запуск серверов с проектами. Список проектов, которые необходимо запустить находится в параметре ApplicationsToRun
		/// Под названием проекта подразумевается его namespace.
		/// </summary>
		protected void StartServer()
		{

			WebServers = new Dictionary<int, IISExpressHost>();
			var applicationsToRun = Config.GetParam("ApplicationsToRun");
			var names = applicationsToRun.Split(',');
			var initialPort = Int32.Parse(Config.GetParam("webPort")); ;
			foreach (var name in names) {
				var server = new IISExpressHost(name, initialPort);
				WebServers.Add(initialPort++,server);
			}
			
		}

		/// <summary>
		/// Запуск браузера
		/// </summary>
		protected static void StartBrowser()
		{
			if (Browser != null)
				return;

			var path = Directory.GetDirectories("../../../packages/", "*ChromeDriver*").FirstOrDefault() + "/tools/";
			var chromeOptions = new SeleniumFixture.ChromeOptionsWithPrefs();
			chromeOptions.prefs = new Dictionary<string, object> {
				{ "download.prompt_for_download", "false" },
				{ "download.default_directory", Environment.CurrentDirectory }
			};
			Browser = new ChromeDriver(path, chromeOptions);
			Browser.Manage().Window.Size = new Size(1200, 1000);
		}

		/// <summary>
		/// Остановка браузера
		/// </summary>
		protected static void StopBrowser()
		{
			if (Browser != null)
			{
				Browser.Quit();
				Browser.Dispose();
				Browser = null;
			}
		}

		/// <summary>
		/// Остановка тестируемых приложений
		/// </summary>
		protected void StopServer()
		{
			WebServers.ForEach(i => i.Value.Dispose());
		}
	}
}