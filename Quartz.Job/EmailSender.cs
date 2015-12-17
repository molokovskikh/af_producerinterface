using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net.Mail;

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
				var a = new Attachment(path);
				message.Attachments.Add(a);
				var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
				using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"], smtpPort)) {
					client.UseDefaultCredentials = false;
					client.Send(message);
				}
			}
		}
	}
}