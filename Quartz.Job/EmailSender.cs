using Quartz.Job.EDM;
using Quartz.Job.Models;
using StringFormat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc.Html;

namespace Quartz.Job
{
	public class EmailSender
	{
		// https://msdn.microsoft.com/ru-ru/library/x5x13z6h(v=vs.110).aspx - async

		public static void SendEmail(List<string> to, string subject, string body, string path)
		{
			foreach (var s in to)
				SendEmail(s, subject, body, path);
		}

		public static void SendEmail(string to, string subject, string body, string path)
		{
			var maFrom = new MailAddress(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["MailFromSubscription"], System.Text.Encoding.UTF8);
			var maTo = new MailAddress(to);

			using (var message = new MailMessage(maFrom, maTo)) {
				message.Subject = subject;
				message.SubjectEncoding = System.Text.Encoding.UTF8;
				message.Body = body;
				message.BodyEncoding = System.Text.Encoding.UTF8;

				message.IsBodyHtml = false;
				if (!String.IsNullOrEmpty(path)) {
					var a = new Attachment(path);
					message.Attachments.Add(a);
				}
				var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
				using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"], smtpPort)) {
					client.UseDefaultCredentials = false;
					client.Send(message);
				}
			}
		}

		public static void SendReportErrorMessage(reportData cntx, long userId, string reportName, string jobName, string errorMessage)
		{
			// TODO при cron-запуске есть вероятность, что пользователя уже нет. Возможно, Главный пользователь Производителя
			var user = cntx.usernames.Single(x => x.UserId == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ReportError);
			var subject = TokenStringFormat.Format(mailForm.Subject, new {SiteName = siteName});
			var body = $"{TokenStringFormat.Format(mailForm.Body, new {ReportName = reportName})}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(user.Email, subject, body, null);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.UserName} (id={user.UserId}, {user.Email}), изготовитель {user.ProducerName} (id={user.ProducerId}), время {DateTime.Now}, отчёт \"{reportName}\", задача {jobName}, сообщение об ошибке {errorMessage}";
      var mailError = ConfigurationManager.AppSettings["MailError"];
			EmailSender.SendEmail(mailError, subject, bodyExtended, null);
		}

		public static void SendRegistrationMessage(reportData cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.Registration, ip);
		}

		public static void SendPasswordChangeMessage(reportData cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.PasswordChange, ip);
		}

		public static void SendPasswordRecoveryMessage(reportData cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.PasswordRecovery, ip);
		}

		private static void SendPasswordMessage(reportData cntx, long userId, string password, MailType type, string ip)
		{
			// TODO при cron-запуске есть вероятность, что пользователя уже нет. Возможно, Главный пользователь Производителя
			var user = cntx.usernames.Single(x => x.UserId == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)type);
			var subject = TokenStringFormat.Format(mailForm.Subject, new {SiteName = siteName});
			var body = $"{TokenStringFormat.Format(mailForm.Body, new {Password = password})}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(user.Email, subject, body, null);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.UserName} ({user.Email}), изготовитель {user.ProducerName}, время {DateTime.Now}, IP {ip}, действие {GetEnumDisplayName(type)}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, bodyExtended, null);
		}

		private static string GetEnumDisplayName(MailType type)
		{
			string displayName = null;
			foreach (var item in EnumHelper.GetSelectList(typeof(MailType), type)) {
				if (item.Selected)
					displayName = item.Text;
			}
			return displayName;
		}
	}
}