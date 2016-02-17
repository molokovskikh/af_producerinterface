using ProducerInterfaceCommon.Models;
using Common.Logging;
using System;
using ProducerInterfaceCommon.ContextModels;
using System.Linq;
using Quartz;
using ProducerInterfaceCommon.Heap;
namespace ProducerInterfaceCommon.Heap
{
	public class ReportJob : IJob
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportJob));

		public void Execute(IJobExecutionContext context)
		{
			var key = context.JobDetail.Key;

			// jparam хранит параметры отчета, неспецифические к моменту запуска
			var jparam = (Report)context.JobDetail.JobDataMap["param"];
			// tparam хранит временнЫе параметы
			var tparam = (TriggerParam)context.Trigger.JobDataMap["tparam"];
			if (tparam is IInterval && jparam is IInterval) {
				((IInterval)jparam).DateFrom = ((IInterval)tparam).DateFrom;
				((IInterval)jparam).DateTo = ((IInterval)tparam).DateTo;
			}
			// TODO при появлении неинтервальных отчетов написать код здесь
			else {
				logger.Error($"Job {key.Group} {key.Name} is not interval report. Mast to be upgrade");
				return;
			}

			logger.Info($"Start running job {key.Group} {key.Name}");

			try {
				jparam.Process(key, jparam, tparam);
				//throw new NotSupportedException("Ошибка!");
			}
			catch (Exception e) {
				logger.Error($"Job {key.Group} {key.Name} run failed:" + e.Message, e);

				var cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
				SetErrorStatus(cntx, key);
				EmailSender.SendReportErrorMessage(cntx, tparam.UserId, jparam.CastomName, key.Name, e.Message);

				return;
			}
			logger.Info($"Job {key.Group} {key.Name} run finished");
		}

		private void SetErrorStatus(ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx, JobKey key)
		{
				// вытащили расширенные параметры задачи
				var jext = cntx.jobextend.Single(x => x.JobName == key.Name
																							&& x.JobGroup == key.Group
																							&& x.Enable == true);

				// отправили статус об ошибке отчета
				jext.DisplayStatusEnum = DisplayStatus.Error;
				jext.LastRun = DateTime.Now;
				cntx.SaveChanges();
		}
	}
}