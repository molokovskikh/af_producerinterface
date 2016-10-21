using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Common.Logging;
using NHibernate;
using NHibernate.Linq;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Helpers;
using ProducerInterfaceCommon.Models;
using Quartz;

namespace ProducerInterfaceCommon.TasksManager.Services
{
	public class EmailNotifierOldReports : BaseTaskManagerService
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (EmailNotifierOldReports));

		public override void ServiceRun(ISession dbSession)
		{
			try {
				var time = SystemTime.Now();
				var term = Int32.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
				var href = ConfigurationManager.AppSettings["UrlToTheReportList"];

				//if (!dbSession.Transaction.IsActive)
				//	dbSession.BeginTransaction();

				var uselessReports =
					dbSession.Query<Job>().Fetch(x => x.Owner).Fetch(x => x.Producer)
						.Where(x => x.SchedName == ReportHelper.GetSchedulerName()
							&& x.Enable && x.LastRun.HasValue && x.LastRun.Value < time.AddMonths(-term).DateTime)
						.ToList();
				var message =
					$"Здравствуйте! На сайте Аналит Формация имеются Ваши отчеты, не использовавшиеся уже более полугода. <br />Вероятно, их необходимо <a href='{href}'>удалить</a>.";
				foreach (var item in uselessReports) {
					if (item.Owner.Login.IndexOf("@") != -1) {
						EmailSender.SendEmail(item.Owner.Login, "Неиспользуемые отчеты", message, new List<string>(), true);
					} else
					{
						logger.Error($"Ошибка при отправке письма с темой 'Неиспользуемые отчеты' для пользователя '{item.Owner.Login}', задача '{this.GetType().Name}'");
					} 
				}
			} catch (Exception e) {
				logger.Error($"Ошибка при отработке задачи '{this.GetType().Name}'", e);
				throw;
			}
		}
	}
}