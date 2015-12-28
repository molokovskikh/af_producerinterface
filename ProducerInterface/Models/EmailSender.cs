using System;
using System.Configuration;
using System.Net.Mail;

namespace ProducerInterface.Models
{
	public class EmailSender
	{
		public static void SendEmail(string[] to, string subject, string body)
		{
			foreach (var s in to) SendEmail(s, subject, body);
		}

		public static void SendEmail(string to, string subject, string body)
		{
			var mail = new MailMessage();
#if DEBUG
			mail.To.Add(ConfigurationManager.AppSettings["DebugInfoEmail"].ToString());
#else
			mail.To.Add(to);
#endif

			mail.From = new MailAddress(ConfigurationManager.AppSettings["MailSenderAddress"].ToString());
#if DEBUG
			mail.Subject = "Moscow Debug - " + subject;
#else
			mail.Subject = subject;		
#endif
			mail.Body = body;
			mail.IsBodyHtml = true;
			SmtpClient smtp = new SmtpClient();
            smtp.Host = ConfigurationManager.AppSettings["SmtpServer"].ToString();
            smtp.Port = 25;
			smtp.UseDefaultCredentials = false;
			smtp.Send(mail);
		}

		public static void SendError(string message)
		{
			var service = ConfigurationManager.AppSettings["ErrorEmail"].ToString();
            if (string.IsNullOrEmpty(service)) {
				return;
			}
			message = "<pre>" + message + "</pre>";
			var mail = new MailMessage();
			mail.To.Add(service);
			mail.From = new MailAddress(service);
			mail.Subject = "Ошибка в Inforoom2";
			mail.Body = message;
			mail.IsBodyHtml = true;
			SmtpClient smtp = new SmtpClient();
			smtp.Host = ConfigurationManager.AppSettings["SmtpServer"].ToString();
            smtp.Port = 25;
			smtp.UseDefaultCredentials = false;
			try
			{
				smtp.Send(mail);
			}
			catch (Exception e)
			{
				// ignore
			}
		}

		public static void SendDebugInfo(string title, string body)
		{
			title = "DebugInfo: " + title;
            var email = ConfigurationManager.AppSettings["DebugInfoEmail"].ToString();
            if (string.IsNullOrEmpty(email)) {
				return;
			}
			try {
				SendEmail(email, title, body);
			}
			catch (Exception e) {
                // ignore
			}
		}
	}
}