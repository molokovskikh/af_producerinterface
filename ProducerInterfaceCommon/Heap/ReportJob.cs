﻿using ProducerInterfaceCommon.Models;
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
			var jparam = (Report)context.JobDetail.JobDataMap["param"];
			// tparam хранит временнЫе параметы
			var tparam = (TriggerParam)context.Trigger.JobDataMap["tparam"];
			if (tparam is IInterval && jparam is IInterval) {
				((IInterval)jparam).DateFrom = ((IInterval)tparam).DateFrom;
				((IInterval)jparam).DateTo = ((IInterval)tparam).DateTo;
			}
			else
				((INotInterval)jparam).DateFrom = ((INotInterval)tparam).DateFrom;

			logger.Info($"Start running job {key.Group} {key.Name}");

			try {
				var processor = jparam.GetProcessor();
				processor.Process(key, jparam, tparam);
			}
			catch (Exception e) {
				logger.Error($"Job {key.Group} {key.Name} run failed:" + e.Message, e);

				var cntx = new producerinterface_Entities();
				SetErrorStatus(cntx, key);
				EmailSender.SendReportErrorMessage(cntx, tparam.UserId, jparam.CastomName, key.Name, e.Message);

				return;
			}
			logger.Info($"Job {key.Group} {key.Name} run finished");
		}

		private void SetErrorStatus(producerinterface_Entities cntx, JobKey key)
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