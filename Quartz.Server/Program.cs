using System;
using System.Configuration;
using Common.Logging.Configuration;
using log4net;
using Topshelf;

namespace Quartz.Server
{
	public static class Program
	{
		public const string DefaultServiceName = "QuartzServer";
		public const string DefaultServiceDisplayName = "Quartz Server";
		public const string DefaultServiceDescription = "Quartz Job Scheduling Server";
		public const string PrefixServerConfiguration = "quartz.server";
		public const string KeyServiceName = PrefixServerConfiguration + ".serviceName";
		public const string KeyServiceDisplayName = PrefixServerConfiguration + ".serviceDisplayName";
		public const string KeyServiceDescription = PrefixServerConfiguration + ".serviceDescription";

		static ILog logger = LogManager.GetLogger(typeof(Program));

		public static int Main()
		{
			try {
				var configuration = (NameValueCollection)ConfigurationManager.GetSection("quartz");
				HostFactory.Run(x => {
					x.DependsOn("Dnscache");
					x.DependsOn("Tcpip");
					x.RunAsLocalSystem();

					x.SetDescription(GetConfigurationOrDefault(configuration, KeyServiceDescription, DefaultServiceDescription));
					x.SetDisplayName(GetConfigurationOrDefault(configuration, KeyServiceDisplayName, DefaultServiceDisplayName));
					x.SetServiceName(GetConfigurationOrDefault(configuration, KeyServiceName, DefaultServiceName));
					x.Service(factory => new QuartzServer());
				});
				return 0;
			} catch(Exception e) {
				logger.Error("Fail on application start", e);
				return 1;
			}
		}

		public static string GetConfigurationOrDefault(NameValueCollection config, string configurationKey, string defaultValue)
		{
			string retValue = null;
			if (config != null) {
				retValue = config[configurationKey];
			}

			if (retValue == null || retValue.Trim().Length == 0) {
				retValue = defaultValue;
			}
			return retValue;
		}
	}
}