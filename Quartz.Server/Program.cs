using System.ComponentModel;
using Quartz.Server.BackgroundServices;
using Topshelf;

namespace Quartz.Server
{
	/// <summary>
	/// The server's main entry point.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Main.
		/// </summary>
		public static void Main()
		{
			// change from service account's dir to more logical one
			//Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

			var hh = System.AppDomain.CurrentDomain.BaseDirectory;

			HostFactory.Run(x => {
				x.DependsOn("Dnscache");
				x.DependsOn("Tcpip");
				x.RunAsLocalSystem();
				//x.RunAsNetworkService();

				x.SetDescription(Configuration.ServiceDescription);
				x.SetDisplayName(Configuration.ServiceDisplayName);
				x.SetServiceName(Configuration.ServiceName);

				x.Service(factory => {
					QuartzServer server = QuartzServerFactory.CreateServer();
					server.Initialize();
					return server;
				});
			});


			HostFactory.Run(x => {
				x.Service<ServiceManager>(s => {
					s.ConstructUsing(name => new ServiceManager());
					s.WhenStarted((os, hostControl) => os.Start(hostControl));
					s.WhenStopped((os, hostControl) => os.Stop(hostControl));
				});
				x.SetDescription(ServiceManager.ServiceDescription);
				x.SetServiceName(ServiceManager.ServiceName);
				x.SetDisplayName(ServiceManager.ServiceDisplayName);

				x.RunAsLocalSystem();
			});


		}
	}
}