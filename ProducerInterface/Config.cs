using AnalitFramefork;
using AnalitFramefork.Components;
using ProducerInterface.Models;

namespace ProducerInterface
{
	public class Config : GlobalConfig
	{
		new public static bool EnableCrudListener = true;
        new public static string UnAuthorizedUserRout = "Registration/Index";
        new public static string HasNoRightUserRout = "Home/Index";
        new public static string MailSenderAddress = "office@analit.net";
        new public static string SiteName = "АналитФармация";
	}
}