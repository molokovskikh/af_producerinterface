using AnalitFramefork;
using AnalitFramefork.Components;
using ProducerInterface.Models;

namespace ProducerInterface
{
	public class Config : GlobalConfig
	{
		public static bool EnableCrudListener = true;
		public static string UnAuthorizedUserRout = "Registration/Index";
		public static string HasNoRightUserRout = "Home/Index";
		public static string MailSenderAddress = "office@analit.net";
		public static string SiteName = "АналитФармация";
	}
}