using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using Castle.Components.DictionaryAdapter;
using Common.Logging;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Tools.Threading;
using ProducerInterfaceCommon.Heap;
using Topshelf;

namespace Quartz.Server.BackgroundServices
{
	public class ServiceManager : ServiceControl
	{
		private bool NeedToBeCanceled { get; set; }
		private RepeatableCommand CurrentComman { get; set; }


		public static string ServiceDescription => "ServiceManager runs ProducereInterface tasks";

		public static string ServiceName => "ServiceManager";

		public static string ServiceDisplayName => "BackgroundServiceManager";

		private static readonly ILog logger = LogManager.GetLogger(typeof (ServiceManager));

		public bool Start(HostControl hostControl)
		{
			ServicesRun();
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			ServicesStop();
			return true;
		}

		private void ServicesRun()
		{
			var tasks = Assembly.GetAssembly(typeof (ServiceManager)).GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && typeof (IBackgroundService).IsAssignableFrom(t))
				.Select(Activator.CreateInstance)
				.OfType<IBackgroundService>()
				.ToArray();

			CurrentComman = new RepeatableCommand(30.Minute(), () => tasks.Each(t => {
				try {
					t.Execute();
					t.Cancellation = CurrentComman.Cancellation;
				} catch (Exception e) {
					logger.Error("Ошибка в Background ProducerInterface", e);
				}
			}));
			CurrentComman.Start();
		}

		private void ServicesStop()
		{
			CurrentComman.Stop();
		}
	}
}