using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.ContextModels;
using StringFormat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc.Html;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace ProducerInterfaceCommon.Heap
{
	public class EmailSender
	{

		public static void SendEmail(List<string> to, string subject, string body, List<string> attachments)
		{
			foreach (var s in to)
				SendEmail(s, subject, body, attachments);
		}

		public static void SendEmail(string to, string subject, string body, List<string> attachments, bool HtmlBody = false)
		{
			var maFrom = new MailAddress(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["MailFromSubscription"], Encoding.UTF8);
			var maTo = new MailAddress(to);

			using (var message = new MailMessage(maFrom, maTo))
			{
				message.Subject = subject;
				message.SubjectEncoding = Encoding.UTF8;
				message.Body = body;
				message.BodyEncoding = Encoding.UTF8;
				message.IsBodyHtml = false;

				if (HtmlBody)
					message.IsBodyHtml = HtmlBody;

				if (attachments != null && attachments.Count > 0)
					foreach (var attachment in attachments)
					{
						message.Attachments.Add(new Attachment(attachment));
					}
				var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
				using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"], smtpPort))
				{
					client.UseDefaultCredentials = false;
					client.Send(message);
				}
			}
		}

		// Автоматическая рассылка отчетов, пользователю и расширенное сотрудникам
		public static void AutoPostReportMessage(producerinterface_Entities cntx, long userId, jobextend jext, string path, List<string> mailTo, string ip)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var creator = cntx.Account.Single(x => x.Id == jext.CreatorId);
			var producerName = cntx.producernames.Single(x => x.ProducerId == jext.ProducerId).ProducerName;

			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.AutoPostReport);
			var subject = ReliableTokenizer(mailForm.Subject, new { ReportName = jext.CustomName, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { ReportName = jext.CustomName, CreatorName = creator.Name, ProducerName = producerName, DateTimeNow = DateTime.Now })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, MailType.AutoPostReport);
			attachments.Add(path);
      EmailSender.SendEmail(mailTo, subject, body, attachments);

			var di = new DiagnosticInformation() {ReportId = jext.JobName, ReportName = jext.CustomName, ProducerId = jext.ProducerId, ProducerName = producerName, Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(MailType.AutoPostReport) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), attachments);
		}

		// Ручная рассылка отчетов, пользователю и расширенное сотрудникам
		public static void ManualPostReportMessage(producerinterface_Entities cntx, long userId, jobextend jext, string path, List<string> mailTo, string ip)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var creator = cntx.Account.Single(x => x.Id == jext.CreatorId);
			var producerName = cntx.producernames.Single(x => x.ProducerId == jext.ProducerId).ProducerName;

			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ManualPostReport);
			var subject = ReliableTokenizer(mailForm.Subject, new { ReportName = jext.CustomName, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { ReportName = jext.CustomName, CreatorName = creator.Name, ProducerName = producerName, DateTimeNow = jext.LastRun, UserName = user.Name, UserLogin = user.Login })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, MailType.ManualPostReport);
			attachments.Add(path);
			EmailSender.SendEmail(mailTo, subject, body, attachments);

			var di = new DiagnosticInformation() { ReportId = jext.JobName, ReportName = jext.CustomName, ProducerId = jext.ProducerId, ProducerName = producerName, Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(MailType.ManualPostReport) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), attachments);
		}

		// Нет данных для формировании отчета, пользователю и расширенное сотрудникам
		public static void SendEmptyReportMessage(producerinterface_Entities cntx, long userId, jobextend jext, string ip)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.EmptyReport);
			var subject = ReliableTokenizer(mailForm.Subject, new { ReportName = jext.CustomName, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { ReportName = jext.CustomName })}\r\n\r\n{mailForm.Footer}";
			var di = new DiagnosticInformation() { ReportId = jext.JobName, ReportName = jext.CustomName, ProducerId = jext.ProducerId, Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(MailType.EmptyReport) };
			var bodyExtended = di.ToString(cntx);
			var attachments = GetAttachments(cntx, MailType.EmptyReport);
			// #48817 пользователю Дополнительная информация высылается также
			EmailSender.SendEmail(user.Login, subject, bodyExtended, attachments);

			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, bodyExtended, attachments);
    }

		// Ошибка при формировании отчета, пользователю и расширенное сотрудникам
		public static void SendReportErrorMessage(producerinterface_Entities cntx, long userId, jobextend jext, string errorMessage, string ip)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ReportError);
			var subject = ReliableTokenizer(mailForm.Subject, new { ReportName = jext.CustomName, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });

			var asdd = ReliableTokenizer(mailForm.Subject, new { ReportName = jext.CustomName, SiteName = siteName });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { ReportName = jext.CustomName })}\r\n\r\n{mailForm.Footer}";

			var di = new DiagnosticInformation() {ErrorMessage = errorMessage, ReportId = jext.JobName, ReportName = jext.CustomName, ProducerId = jext.ProducerId, Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(MailType.ReportError) };
			var bodyExtended = di.ToString(cntx);
			var attachments = GetAttachments(cntx, MailType.ReportError);
			// #48817 пользователю Дополнительная информация высылается также
			EmailSender.SendEmail(user.Login, subject, bodyExtended, attachments);

      var mailError = ConfigurationManager.AppSettings["MailError"];
			EmailSender.SendEmail(mailError, subject, bodyExtended, attachments);
		}

		// Регистрация в системе, пользователю и расширенное сотрудникам
		public static void SendRegistrationMessage(producerinterface_Entities cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.Registration, ip);
		}

		// Смена пароля, пользователю и расширенное сотрудникам
		public static void SendPasswordChangeMessage(producerinterface_Entities cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.PasswordChange, ip);
		}

		// Восстановление пароля, пользователю и расширенное сотрудникам
		public static void SendPasswordRecoveryMessage(producerinterface_Entities cntx, Int64 userId, string password, string ip)
		{
			SendPasswordMessage(cntx, userId, password, MailType.PasswordRecovery, ip);
		}

		public static void SendControlPanelRegistrationSuccessMessage(producerinterface_Entities cntx, Int64 userId, string password, string ip, long adminId)
		{

			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.RegistrationSuccess);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { Password = password })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, MailType.RegistrationSuccess);
			EmailSender.SendEmail(user.Login, subject, body, attachments, true);

			var di = new DiagnosticInformation() { AdminLogin = cntx.Account.Find(adminId).Login, Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(MailType.RegistrationSuccess) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), attachments, false);
		}

		// Универсальное на смену пароля, пользователю и расширенное сотрудникам
		private static void SendPasswordMessage(producerinterface_Entities cntx, long userId, string password, MailType type, string ip)
		{
			// TODO при cron-запуске есть вероятность, что пользователя уже нет. Возможно, Главный пользователь Производителя
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)type);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { Password = password })}\r\n\r\n{mailForm.Footer}";
			var bodyWithoutPassword = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { Password = "*******" })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, type);
			EmailSender.SendEmail(user.Login, subject, body, attachments, false);

			var di = new DiagnosticInformation() { Body = bodyWithoutPassword, User = user, UserIp = ip, ActionName = GetEnumDisplayName(type) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), attachments, false);
		}

		// Изменение ПКУ, сотрудникам
		public static void SendCatalogChangeMessage(producerinterface_Entities cntx, Account user, string field, string catalogName, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			var subject = $"Изменение ПКУ препарата на сайте {siteName}";
			var body = $"Изменено свойство {field} формы выпуска {catalogName}\r\n\r\nБыло: {before}\r\n\r\nСтало: {after}";

			var di = new DiagnosticInformation() { Body = body, User = user, UserIp = user.IP, ActionName = "Изменение ПКУ препарата" };
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Изменение описания препарата, сотрудникам
		public static void SendDescriptionChangeMessage(producerinterface_Entities cntx, Account user, string field, string drugFamilyName, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			var subject = $"Изменение описания препарата на сайте {siteName}";
			var body = $"Изменено поле {field} препарата {drugFamilyName}\r\n\r\nБыло:\r\n\r\n{before}\r\n\r\nСтало:\r\n\r\n{after}";

			var di = new DiagnosticInformation() { Body = body, User = user, UserIp = user.IP, ActionName = "Изменение описания препарата" };
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Именение МНН препарата, сотрудникам
		public static void SendMnnChangeMessage(producerinterface_Entities cntx, Account user, string drugFamilyName, string mnnBefore, string mnnAfter)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			var subject = $"Изменение МНН препарата на сайте {siteName}";
			var body = $"Изменен МНН препарата {drugFamilyName}\r\n\r\nБыло:\r\n\r\n{mnnBefore}\r\n\r\nСтало:\r\n\r\n{mnnAfter}";

			var di = new DiagnosticInformation() { Body = body, User = user, UserIp = user.IP, ActionName = "Изменение МНН препарата" };
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Обратная связь, сотрудникам
		public static void SendFeedBackMessage(producerinterface_Entities cntx, Account user, string message, string Ip)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			var subject = $"Сообщение пользователя с сайта {siteName}";

			var di = new DiagnosticInformation() { Body = message, User = user, UserIp = Ip, ActionName = "Обратная связь" };
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), null, false);
		}


		// Создание акции, пользователю и расширенное сотрудникам
		public static void SendNewPromotion(producerinterface_Entities cntx, long userId, long promotionId, string ip)
		{
			var mailType = MailType.CreatePromotion;
      var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var promotion = cntx.promotions.Single(x => x.Id == promotionId);
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)mailType);
			var subject = ReliableTokenizer(mailForm.Subject, new { PromotionName = promotion.Name, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { PromotionName = promotion.Name, UserName = user.Name, UserLogin = user.Login, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, mailType);
			EmailSender.SendEmail(user.Login, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", attachments, true);

			var di = new DiagnosticInformation() { Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(mailType) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + di.ToString(cntx) + "</p>", attachments, true);
		}

		// Изменение акции, пользователю и расширенное сотрудникам
		public static void SendChangePromotion(producerinterface_Entities cntx, long userId, long PromotionId, string ip)
		{
			var mailType = MailType.EditPromotion;
      var user = cntx.Account.Single(x => x.Id == userId);
			long userCreatePromotionID = cntx.promotions.Single(x => x.Id == PromotionId).ProducerUserId;
			var sendEmailOnCreateUserPromotion = cntx.Account.Single(xxx => xxx.Id == userCreatePromotionID);
			var emailCreateUser = cntx.promotions.Where(xxx => xxx.Id == PromotionId).Select(yyy => yyy.ProducerUserId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var promotion = cntx.promotions.Single(x => x.Id == PromotionId);
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)mailType);
			var subject = ReliableTokenizer(mailForm.Subject, new { PromotionName = promotion.Name, SiteName = siteName });
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { PromotionName = promotion.Name, UserName = user.Name, UserLogin = user.Login, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, mailType);
			EmailSender.SendEmail(sendEmailOnCreateUserPromotion.Login, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", attachments, true);

			var di = new DiagnosticInformation() { Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(mailType) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + di.ToString(cntx) + "</p>", attachments, true);
		}

		// Подтверждение акции, пользователю и расширенное сотрудникам
		public static void SendPromotionStatus(producerinterface_Entities cntx, long userId, long promotionId, string ip)
		{
			var mailType = MailType.StatusPromotion;
      var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var promotion = cntx.promotions.Single(x => x.Id == promotionId);
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];
			var sendEmailOnCreateUserPromotion = cntx.Account.Where(xxx => xxx.CompanyId == cntx.promotions.Single(yyy => yyy.Id == promotionId).ProducerUserId).Select(zzz => zzz.Login).First();
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)mailType);
			var subject = ReliableTokenizer(mailForm.Subject, new { PromotionName = promotion.Name, SiteName = siteName });
			string statusPromotion = "Деактивирована";
			if (promotion.Status) { statusPromotion = "Подтверждена"; }
			var header = ReliableTokenizer(mailForm.Header, new { UserName = user.Name });
			var body = $"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new { PromotionName = promotion.Name, Status = statusPromotion, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			var attachments = GetAttachments(cntx, mailType);
			EmailSender.SendEmail(sendEmailOnCreateUserPromotion, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", attachments, true);

			var di = new DiagnosticInformation() {Body = body, User = user, UserIp = ip, ActionName = GetEnumDisplayName(mailType) };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + di.ToString(cntx) + "</p>", attachments, true);
		}

		private static string GetEnumDisplayName(MailType type)
		{
			string displayName = null;
			foreach (var item in EnumHelper.GetSelectList(typeof(MailType), type))
			{
				if (item.Selected)
					displayName = item.Text;
			}
			return displayName;
		}

		private static List<string> GetAttachments(producerinterface_Entities cntx, MailType mailType)
		{
			var result = new List<string>();

			// общая директория медиафайлов
			var dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "MediaFiles");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			// поддиректория для хранения файлов данного типа писем
			var subdir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "MediaFiles", mailType.ToString());
			if (!Directory.Exists(subdir))
				Directory.CreateDirectory(subdir);

			// записали файлы в директорию
			var mediaFiles = cntx.mailform.Single(x => x.Id == (int)mailType).MediaFiles.Select(x => new { x.Id, x.ImageName }).ToList();
			foreach (var mediaFile in mediaFiles)
			{
				var file = new FileInfo($"{subdir}\\{mediaFile.ImageName}");
				if (!file.Exists)
				{
					var bdFile = cntx.MediaFiles.Single(x => x.Id == mediaFile.Id).ImageFile;
					File.WriteAllBytes(file.FullName, bdFile);
				}
				result.Add(file.FullName);
			}
			return result;
		}

		// исходное поведение: валится, если не приходит требующейся подстановки. Исправлено: просто не подставляется, но не валится
		private static string ReliableTokenizer(string format, object substitution)
		{
			// получили лист всех имеющихся в формате подстановок
			IEnumerable<string> tokens;
			TokenStringFormat.TokenizeString(format, out tokens);
			var list = (List<string>)tokens;

			// превратили объект подстановок в словарь
			var dictionary = new Dictionary<string, object>();
			if (substitution != null)
			{
				foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(substitution))
					dictionary.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(substitution));
			}

			// все, что требуется, но не содержится в словаре
			var required = list.Where(x => !dictionary.ContainsKey(x)).Select(x => x).ToList();

			// добавили в словарь то, что требуется, но чего нет
			foreach (var r in required)
				dictionary.Add(r, null);

			return TokenStringFormat.Format(format, dictionary);
		}

		private class DiagnosticInformation
		{
			public string Body { get; set; }
			public Account User { get; set; }
			public string UserIp { get; set; }
			public DateTime? ActionDate { get; set; }
			public string ActionName { get; set; }
			public long? ProducerId { get; set; }
			public string ProducerName { get; set; }
			public string ReportId { get; set; }
			public string ReportName { get; set; }
			public string ErrorMessage { get; set; }
			public string AdminLogin { get; set; }

			public string ToString(producerinterface_Entities cntx)
			{
				var sb = new StringBuilder();

				// может быть незарегистрированный пользователь
				if (User != null)
				{ 
					// у админов AccountCompany is null
					var ac = User.AccountCompany;
					if (ProducerId == null && ac != null && ac.ProducerId != null)
						ProducerId = ac.ProducerId;

					if (string.IsNullOrEmpty(ProducerName) && ProducerId != null) {
						var producer = cntx.producernames.SingleOrDefault(x => x.ProducerId == ac.ProducerId);
						if (producer != null)
							ProducerName = producer.ProducerName;
					}

					// у новых пользоватей может не быть производителя, если они не нашли его в системе, а указали его
					if (string.IsNullOrEmpty(ProducerName) && ProducerId == null && ac != null)
						ProducerName = ac.Name;
				}

				if (!ActionDate.HasValue)
					ActionDate = DateTime.Now;

				sb.AppendLine(Body);
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Дополнительная информация");
				if (User != null)
				{
					sb.AppendLine($"пользователь: {User.Name} (id={User.Id}, {User.Login}, IP {UserIp})");
					sb.AppendLine($"производитель: {ProducerName} (id={ProducerId})");
				}
				else
					sb.AppendLine($"Незарегистрированный пользователь (IP {UserIp})");
				sb.AppendLine($"действие: {ActionName}, время {ActionDate}");
				if (!string.IsNullOrEmpty(ReportId))
					sb.AppendLine($"отчет: {ReportName} (id={ReportId})");
				if (!string.IsNullOrEmpty(ErrorMessage))
					sb.AppendLine($"сообщение об ошибке: {ErrorMessage}");
				if (!string.IsNullOrEmpty(AdminLogin))
					sb.AppendLine($"письмо отправлено из Панели управления администратором: {AdminLogin}");

				return sb.ToString();
			}
		}
	}
}