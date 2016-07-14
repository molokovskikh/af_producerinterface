using System.Security.Cryptography;
using System.Text;

namespace ProducerInterfaceCommon.Helpers
{
	public class Md5HashHelper
	{
		/// <summary>
		/// ��������� ���� ������
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetHash(string text)
		{
			using (MD5 md5Hash = MD5.Create()) return GetMd5Hash(md5Hash, text);
		}

		/// <summary>
		/// ��������� ���� ������
		/// </summary>
		/// <param name="md5Hash">������ MD5</param>
		/// <param name="text">���������� ������</param>
		/// <returns>��� ������</returns>
		public static string GetMd5Hash(MD5 md5Hash, string text)
		{
			// ����������� ��������� ������� � ���
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
			// �������� ������
			StringBuilder sBuilder = new StringBuilder();
			// �������� �� ������� ����� ���� � ����������� ��� � 16 string
			for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
			return sBuilder.ToString();
		}
	}
}