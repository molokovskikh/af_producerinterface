using System;
using System.Linq;
using System.Reflection;
using AnalitFramefork;
using AnalitFramefork.Components;
using AnalitFramefork.Hibernate.Models;
using ProducerControlPanel.Models;

namespace ProducerControlPanel
{
	public class Config : GlobalConfig
	{
		public static bool EnableCrudListener = true;
	}
}