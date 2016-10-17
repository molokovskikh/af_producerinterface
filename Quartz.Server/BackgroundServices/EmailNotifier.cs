using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Castle.ActiveRecord;
using Castle.Components.DictionaryAdapter;
using log4net;
using NHibernate;
using NHibernate.Linq;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Helpers;

namespace Quartz.Server.BackgroundServices
{
	public class EmailNotifier : IBackgroundService
	{
		private ILog log = log4net.LogManager.GetLogger(typeof (EmailNotifier));

		public CancellationToken Cancellation { get; set; }

		public int RepeatInterval => 30;

		public void Execute()
		{
			SendMails_ReportLongTermDownTime();
		}

		private void SendMails_ReportLongTermDownTime()
		{
			var time = SystemTime.Now();
			var dayOfMonth = ConfigurationManager.AppSettings["TimeForMailDayOfMonth_ReportLongTermDown"];
			if (!string.IsNullOrEmpty(dayOfMonth) && Int32.Parse(dayOfMonth) != time.Day)
				return;
			var timeToSendMails = ConfigurationManager.AppSettings["TimeForMail24_ReportLongTermDown"].Split(':');
			var hour = Int32.Parse(timeToSendMails[0]);
			var minutes = Int32.Parse(timeToSendMails[1]);
			if (time.Hour == hour && time.Minute >= minutes && time.Minute < minutes + RepeatInterval) {
				var term = Int32.Parse(ConfigurationManager.AppSettings["DeleteOldReportsTerm"]);
				var href = ConfigurationManager.AppSettings["UrlToTheReportList"];
				var dbSession = ServiceManager.DbFactory.OpenSession();
				try {
					dbSession.BeginTransaction();
					var uselessReports =
						dbSession.Query<ProducerInterfaceCommon.Models.Job>().Fetch(x => x.Owner).Fetch(x => x.Producer)
							.Where(x => x.SchedName == ReportHelper.GetSchedulerName()
								&& x.Enable && x.LastRun.HasValue && x.LastRun.Value < time.AddMonths(-term).DateTime)
							.ToList();
					var message =
						$"Здравствуйте! На сайте Аналит Формация имеются Ваши отчеты, не использовавшиеся уже более полугода. <br />Вероятно, их необходимо <a href='{href}'>удалить</a>.";
					foreach (var item in uselessReports) {
						if (Cancellation.CanBeCanceled) {
							break;
						}
						EmailSender.SendEmail(item.Owner.Login, "Не используемые отчеты", message, new EditableList<string>(), true);
					}
				}  finally {
					dbSession.Close();
				}
			}
		}
	}
}