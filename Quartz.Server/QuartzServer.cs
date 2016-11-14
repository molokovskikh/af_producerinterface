using System;
using Common.Logging;
using Quartz.Impl;
using Topshelf;

namespace Quartz.Server
{
	/// <summary>
	/// The main server logic.
	/// </summary>
	public class QuartzServer : ServiceControl
	{
		private readonly ILog logger;
		private ISchedulerFactory schedulerFactory;
		private IScheduler scheduler;

		/// <summary>
		/// Initializes a new instance of the <see cref="QuartzServer"/> class.
		/// </summary>
		public QuartzServer()
		{
			logger = LogManager.GetLogger(GetType());
			try {
				schedulerFactory = new StdSchedulerFactory();
				scheduler = schedulerFactory.GetScheduler();
			}
			catch (Exception e) {
				logger.Error("Server initialization failed:" + e.Message, e);
				throw;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			try {
				scheduler.Shutdown(false);
			}
			catch (Exception ex) {
				logger.Error(string.Format("Scheduler stop failed: {0}", ex.Message), ex);
				throw;
			}

			scheduler = null;
			logger.Info("Scheduler dispose complete");
		}

		/// <summary>
		/// TopShelf's method delegated to <see cref="Start()"/>.
		/// </summary>
		public bool Start(HostControl hostControl)
		{
			try {
				scheduler.Start();
			}
			catch (Exception ex) {
				logger.Fatal(string.Format("Scheduler start failed: {0}", ex.Message), ex);
				throw;
			}

			logger.Info("Scheduler started successfully");
			return true;
		}

		/// <summary>
		/// TopShelf's method delegated to <see cref="Stop()"/>.
		/// </summary>
		public bool Stop(HostControl hostControl)
		{
			try {
				scheduler.Shutdown(true);
			}
			catch (Exception ex) {
				logger.Error(string.Format("Scheduler stop failed: {0}", ex.Message), ex);
				throw;
			}

			logger.Info("Scheduler shutdown complete");
			return true;
		}

		/// <summary>
		/// TopShelf's method delegated to <see cref="Pause()"/>.
		/// </summary>
		public bool Pause(HostControl hostControl)
		{
			scheduler.PauseAll();
			return true;
		}

		/// <summary>
		/// TopShelf's method delegated to <see cref="Resume()"/>.
		/// </summary>
		public bool Continue(HostControl hostControl)
		{
			scheduler.ResumeAll();
			return true;
		}
	}
}