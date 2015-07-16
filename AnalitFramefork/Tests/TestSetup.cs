using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using CassiniDev;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace AnalitFramefork.Tests
{

	/// <summary>
	/// Загрузчик тестов. Поднимает сервер, запускает хром и так далее.
	/// Необходимо не забыть добавить аттрибут[SetUpFixture] наследнику данного класса
	/// Также нужно убедиться, что проект с тестами находится в x86 архитектуре
	/// </summary>
	[SetUpFixture]
	public class TestSetup
	{
		private static Server _webServer;
		public static RemoteWebDriver Browser;
		public static RemoteWebDriver GlobalDriver;
		public static string defaultUrl = "/";
		public static string WebRoot;
		public static int WebPort;
		public static string WebDir;

		[SetUp]
		public void SetupFixture()
		{
			//Все опасные функции, должны быть вызванны до этого момента, так как исключения в сетапе
			//оставляют невысвобожденные ресурсы браузера и веб сервера
			GlobalSetup();
			_webServer = StartServer();
		}

		[TearDown]
		public void TeardownFixture()
		{
			GlobalTearDown();
			_webServer.ShutDown();
		}

		public Server StartServer()
		{
			WebPort = Int32.Parse(ConfigurationManager.AppSettings["webPort"]);
			WebRoot = ConfigurationManager.AppSettings["webRoot"] ?? "/";
			WebDir = ConfigurationManager.AppSettings["webDirectory"];

			var webServer = new Server(WebPort, WebRoot, Path.GetFullPath(WebDir));
			webServer.Start();

			try
			{
				SetupEnvironment(webServer);
			}
			catch (Exception)
			{
				try
				{
					webServer.Dispose();
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
				}
				throw;
			}

			return webServer;
		}

		public static void SetupEnvironment(Server server)
		{
			var method = server.GetType().GetMethod("GetHost", BindingFlags.Instance | BindingFlags.NonPublic);
			method.Invoke(server, null);

			var manager = ApplicationManager.GetApplicationManager();
			var apps = manager.GetRunningApplications();
			var domain = manager.GetAppDomain(apps.Single().ID);
			domain.SetData("environment", "test");
		}

	

		public static void GlobalSetup()
		{
			WebPort = Int32.Parse(ConfigurationManager.AppSettings["webPort"]);
			WebRoot = ConfigurationManager.AppSettings["webRoot"] ?? "/";
			if (GlobalDriver != null)
				return;

			var path = Directory.GetDirectories("../../../packages/", "*ChromeDriver*").FirstOrDefault() + "/tools/";
			var chromeOptions = new SeleniumFixture.ChromeOptionsWithPrefs();
			chromeOptions.prefs = new Dictionary<string, object> {
				{ "download.prompt_for_download", "false" },
				{ "download.default_directory", Environment.CurrentDirectory }
			};
			GlobalDriver = new ChromeDriver(path, chromeOptions);
			GlobalDriver.Manage().Window.Size = new Size(1200, 1000);
		}

		public static void GlobalTearDown()
		{
			if (GlobalDriver != null)
			{
				GlobalDriver.Quit();
				GlobalDriver.Dispose();
				GlobalDriver = null;
			}
		}
	}
}