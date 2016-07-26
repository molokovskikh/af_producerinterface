using ProducerInterfaceCommon.Models;
using Common.Logging;
using System;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;
using Quartz;

namespace ProducerInterfaceCommon.Heap
{
	public class ReportJob : IJob
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportJob));

		public void Execute(IJobExecutionContext context)
		{
			var key = context.JobDetail.Key;

			// jparam хранит параметры отчета, неспецифические к моменту запуска
			var report = (Report)context.JobDetail.JobDataMap["param"];
			// tparam хранит временнЫе параметы
			var interval = (TriggerParam)context.Trigger.JobDataMap["tparam"];
			if (interval is IInterval && report is IInterval) {
				((IInterval)report).DateFrom = ((IInterval)interval).DateFrom;
				((IInterval)report).DateTo = ((IInterval)interval).DateTo;
			}
			else
				((INotInterval)report).DateFrom = ((INotInterval)interval).DateFrom;

			logger.Info($"Start running job {key.Group} {key.Name}");

			try {
				report.Run(key, interval);
			}
			catch (Exception e) {
				logger.Error($"Job {key.Group} {key.Name} run failed:" + e.Message, e);

				var db = new producerinterface_Entities();
				// вытащили расширенные параметры задачи
				var jext = db.jobextend.Single(x => x.JobName == key.Name
																							&& x.JobGroup == key.Group
																							&& x.Enable);

				// отправили статус об ошибке отчета
				jext.DisplayStatusEnum = DisplayStatus.Error;
				jext.LastRun = DateTime.Now;
				db.SaveChanges();

				var ip = "неизвестен (авт. запуск)";
				if (interval is RunNowParam)
					ip = ((RunNowParam)interval).Ip;

				var user = db.Account.First(x => x.Id == interval.UserId);
				user.IP = ip;
				var mail = new EmailSender(db, new Context(), user);

				mail.SendReportErrorMessage(jext, e.Message);

				return;
			}
			logger.Info($"Job {key.Group} {key.Name} run finished");
		}

	}
}