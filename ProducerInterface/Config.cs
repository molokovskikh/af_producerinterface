using AnalitFramefork;
using AnalitFramefork.Components;
using ProducerInterface.Models;

namespace ProducerInterface
{
	public class Config : GlobalConfig
	{
		public static bool EnableCrudListener = true;
		public static bool RuntimeHibernateMigration = false;
	}
}