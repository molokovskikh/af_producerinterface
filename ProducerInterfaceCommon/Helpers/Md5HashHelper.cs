using System.Security.Cryptography;
using System.Text;

namespace ProducerInterfaceCommon.Helpers
{
	public class Md5HashHelper
	{
		/// <summary>
		/// Получение хэша строки
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetHash(string text)
		{
			using (MD5 md5Hash = MD5.Create()) return GetMd5Hash(md5Hash, text);
		}

		/// <summary>
		/// получение хэша строки
		/// </summary>
		/// <param name="md5Hash">Объект MD5</param>
		/// <param name="text">Хэшируемая строка</param>
		/// <returns>Хыш строки</returns>
		public static string GetMd5Hash(MD5 md5Hash, string text)
		{
			// Конвертация байтового массива в хэш
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
			// создание строки
			StringBuilder sBuilder = new StringBuilder();
			// проходит по каждому байту хэша и форматирует его в 16 string
			for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
			return sBuilder.ToString();
		}
	}
}