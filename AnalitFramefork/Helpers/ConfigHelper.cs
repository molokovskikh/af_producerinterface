using System;

namespace AnalitFramefork.Helpers
{
	/// <summary>
	/// Хелпер для работы с конфигурационными файлами
	/// </summary>
	public class ConfigHelper
	{

		static public void LoadParams()
		{
			
		}

		/// <summary>
		/// Получение параметра из файла конфигурации
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <returns>Значение параметра</returns>
		static public string GetParam(string name)
		{
			var result = System.Web.Configuration.WebConfigurationManager.AppSettings[name];

			if (result == null)
				throw new Exception("Не удалось найти параметр {0} в текущем файле кофигурации");

			return result;
		}

		/// <summary>
		/// Получение параметра из файла конфигурации
		/// </summary>
		/// <param name="name">Имя параметра</param>
		/// <param name="defaultValue">Значение по умолчанию</param>
		/// <returns>Значение параметра</returns>
		static public string GetParam(string name, string defaultValue)
		{
			var result = System.Web.Configuration.WebConfigurationManager.AppSettings[name];

			if (result == null)
				return defaultValue;

			return result;
		}
	}
}