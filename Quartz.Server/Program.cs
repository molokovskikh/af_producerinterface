using log4net;
using log4net.Config;
using System.IO;
using Topshelf;

[assembly: XmlConfigurator(Watch = true)]

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
		}
	}
}