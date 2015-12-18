using Quartz.Job.Models;
using Common.Logging;
using System;

namespace Quartz.Job
{
	public class ReportJob : IJob
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportJob));

		public void Execute(IJobExecutionContext context)
		{
			var key = context.JobDetail.Key;
			// SimpleTrigger запускает джобы сразу, иначе - CronTrigger по расписанию
			var runNow = context.Trigger is ISimpleTrigger;

			var param = (Report)context.JobDetail.JobDataMap["param"];
			logger.Info($"Start running job {key.Group} {key.Name}");

			try {
				param.Process(param, key, runNow);
			}
			catch (Exception e) {
				logger.Error($"Job {key.Group} {key.Name} run failed:" + e.Message, e);
				return;
			}
			logger.Info($"Job {key.Group} {key.Name} run finished");
		}
	}
}