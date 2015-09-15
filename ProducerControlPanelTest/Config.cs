using AnalitFramefork.Components;


namespace ProducerControlPanelTest
{
	public class Config : GlobalConfig
	{
		public static bool EnableCrudListener = false;
		public static bool RuntimeHibernateMigration = false;

		public static string webPort = "9790";
		public static string webDirectory = "ProducerControlPanel";
		public static string ApplicationsToRun = "ProducerControlPanel"; 

	}
}