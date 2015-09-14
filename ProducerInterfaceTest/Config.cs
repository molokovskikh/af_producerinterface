using AnalitFramefork.Components;

namespace ProducerInterfaceTest
{
	public class Config : GlobalConfig
	{
		public static bool EnableCrudListener = false;
		public static bool RuntimeHibernateMigration = false;
	}
}