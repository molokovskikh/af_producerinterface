using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnalitFramefork
{
	public class Config
	{
		/// <summary>
		/// Получение глобальных параметров
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <returns>Значение параметра</returns>
		public static string GetParam(string name)
		{
			var element = Framework.Assembly != null ? Assembly.GetAssembly(Framework.Assembly.DefinedTypes
				.First(d => d.Name == "Config")).GetTypes().Last(t => t.IsSubclassOf(typeof(AnalitFramefork.Config))) : typeof(Config);
			var result = element.GetProperty(name).GetValue(Activator.CreateInstance(element), null);

			if (result == null)
				throw new Exception("Не удалось найти параметр {0} в текущем файле кофигурации");

			return result.ToString();
		}

		/// <summary>
		/// Получение глобальных параметров
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <returns>Значение параметра</returns>
		public static string GetParam(string name, string defaultValue)
		{
			var element = Framework.Assembly != null ? Assembly.GetAssembly(Framework.Assembly.DefinedTypes
				.First(d => d.Name == "Config")).GetTypes().Last(t => t.IsSubclassOf(typeof(AnalitFramefork.Config))) : typeof(Config);
			var result = element.GetProperty(name).GetValue(Activator.CreateInstance(element), null);

			if (result == null)
				return defaultValue;

			return result.ToString();
		}

		public virtual string Test
		{
			get { return "Test -1"; }
		}

		public virtual string SiteRoot
		{
			get { return "http://localhost:56790/"; }
		}

		public virtual string AdminRoot
		{
			get { return "http://localhost:56791/"; }
		}

		public virtual string SiteName
		{
			get { return "SiteName.net"; }
		}

		public virtual string MailSenderAddress
		{
			get { return "ayakimenko@analit.net"; } // internet@ivrn.net
		}

		public virtual string SmtpServer
		{
			get { return "box.analit.net"; }
		}

		public virtual string DebugInfoEmail
		{
			get { return "dev@analit.net"; }
		}

		public virtual string webPort
		{
			get { return "9789"; }
		}
		
		public virtual string webRoot
		{
			get { return "59777"; }
		}
		public virtual string webDirectory
		{
			get { return "../../../ProducerInterface/"; }
		}

		public virtual string ApplicationsToRun
		{
			get { return "ProducerInterface"; }
		}

		public virtual string AnalitEmail
		{
			get { return "office@analit.net"; }
		}

		public virtual string ErrorEmail
		{
			get { return "service@analit.net"; }
		}
		public virtual string NhibernateConnectionString
		{
			get { return "Server=localhost;Database=ProducerInterface;User ID=root;Password=;"; }
		}
		

	}
}