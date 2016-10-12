using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Net;
using StringFormat;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.ContextModels;
using Account = ProducerInterfaceCommon.ContextModels.Account;

namespace ProducerInterfaceCommon.Heap
{
	public class EmailSender
	{
		private producerinterface_Entities db;
		private Context db2;
		private Account currentUser;
		private DiagnosticInformation diagnostics;
		private List<string> attachments = new List<string>();
		private bool isHtml;
		public string IP;

		public EmailSender(producerinterface_Entities db, Context db2,
			Account currentUser)
		{
			this.db = db;
			this.db2 = db2;
			this.currentUser = currentUser;
			diagnostics = new DiagnosticInformation(currentUser);
		}

		public static void SendEmail(List<string> to, string subject, string body, List<string> attachments, bool isHtml = false)
		{
			foreach (var s in to)
				SendEmail(s, subject, body, attachments, isHtml);
		}

		public static void SendEmail(string to, string subject, string body, List<string> attachments, bool isHtml = false)
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

				if (isHtml)
					message.IsBodyHtml = isHtml;

				if (attachments != null && attachments.Count > 0)
					foreach (var attachment in attachments)
					{
						message.Attachments.Add(new Attachment(attachment));
					}
				var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
				using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"], smtpPort))
				{
					client.UseDefaultCredentials = false;
#if DEBUG
					return;
#endif
					client.Send(message);
				}
			}
		}

		// Автоматическая рассылка отчетов, пользователю и расширенное сотрудникам
		public void AutoPostReportMessage(jobextend jext, string path, List<string> mailTo)
		{
			attachments.Add(path);
			MessageForReport(jext, mailTo, MailType.AutoPostReport);
		}

		// Ручная рассылка отчетов, пользователю и расширенное сотрудникам
		public void ManualPostReportMessage(jobextend jext, string path, List<string> mailTo)
		{
			attachments.Add(path);
			MessageForReport(jext, mailTo, MailType.ManualPostReport);
		}

		// Нет данных для формировании отчета, пользователю и расширенное сотрудникам
		public void SendEmptyReportMessage(jobextend jext)
		{
			MessageForReport(jext, new List<string> { currentUser.Login }, MailType.EmptyReport);
		}

		// Ошибка при формировании отчета, пользователю и расширенное сотрудникам
		public void SendReportErrorMessage(jobextend jext, string errorMessage)
		{
			diagnostics.ErrorMessage = errorMessage;
			MessageForReport(jext, new List<string> { currentUser.Login }, MailType.ReportError);
		}

		private void MessageForReport(jobextend jext, List<string> mailTo, MailType type)
		{
			var creator = db.Account.Single(x => x.Id == jext.CreatorId);
			var producerName = db.producernames.FirstOrDefault(x => x.ProducerId == jext.ProducerId)?.ProducerName;

			var args = new {
				ReportName = jext.CustomName,
				CreatorName = creator.Name,
				ProducerName = producerName,
				DateTimeNow = jext.LastRun,
				UserName = currentUser.Name,
				UserLogin = currentUser.Login
			};
			diagnostics.ReportId = jext.JobName;
			diagnostics.ReportName = jext.CustomName;
			diagnostics.ProducerId = jext.ProducerId;
			diagnostics.ProducerName = producerName;
			MessageFromTemplate(type, mailTo, args);
		}

		// Регистрация в системе, пользователю и расширенное сотрудникам
		public void SendRegistrationMessage(Account targetUser, string password)
		{
			MessageForUser(MailType.Registration, targetUser, new { Password = password, });
		}

		// Смена пароля, пользователю и расширенное сотрудникам
		public void SendPasswordChangeMessage(Account targetUser, string password)
		{
			MessageForUser(MailType.PasswordChange, targetUser, new { Password = password, });
		}

		// Восстановление пароля, пользователю и расширенное сотрудникам
		public void SendPasswordRecoveryMessage(Account targetUser, string password)
		{
			MessageForUser(MailType.PasswordRecovery, targetUser, new { Password = password });
		}

		// Подтверждение регистрации пользователя без производителя
		public void SendAccountVerificationMessage(Account targetUser, string password)
		{
			MessageForUser(MailType.AccountVerification, targetUser, new { Password = password });
		}

		public void RegistrationReject(Account targetUser, string comment)
		{
			MessageForUser(MailType.RegistrationRejected, targetUser, new { Comment = comment });
		}

		private void MessageForUser(MailType type, Account targetUser, object args)
		{
			var values = new Dictionary<string, object> {
				{ "UserName", targetUser.Name },
				{ "UserLogin", targetUser.Login },
			};
			MessageFromTemplate(type, targetUser.Login, Merge(values, ToMap(args)));
		}

		private void MessageFromTemplate(MailType type, string to, Dictionary<string, object> args)
		{
			MessageFromTemplate(type, new List<string> { to }, args);
		}

		private void MessageFromTemplate(MailType type, string to, object args)
		{
			MessageFromTemplate(type, new List<string> { to }, ToMap(args));
		}

		private void MessageFromTemplate(MailType type, List<string> to, object args)
		{
			MessageFromTemplate(type, to, ToMap(args));
		}

		private void MessageFromTemplate(MailType type, List<string> to, Dictionary<string, object> args)
		{
			var debugMail = ConfigurationManager.AppSettings["DebugEmail"];
			if (!String.IsNullOrEmpty(debugMail))
				to = new List<string> { debugMail };
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var values = new Dictionary<string, object> {
				{ "SiteName", siteName },
				{ "DateTimeNow", DateTime.Now }
			};
			Merge(values, args);
			var mailForm = db.mailformwithfooter.Single(x => x.Id == (int) type);
			var subject = Render(mailForm.Subject, values);
			var header = Render(mailForm.Header, values);
			var body = Render(mailForm.Body, values);
			var footer = Render(mailForm.Footer, values);
			attachments.AddRange(EmailSender.GetAttachments(db2, type));

			body = $"{header}\r\n\r\n{body}\r\n\r\n{footer}";
			if (isHtml)
				body = "<p style='white-space: pre-wrap;'>" + body + "</p>";
			SendEmail(to, subject, body, attachments, isHtml);

			values["Password"] = "*******";
			body = Render(mailForm.Body, values);
			body = $"{header}\r\n\r\n{body}\r\n\r\n{footer}";
			diagnostics.Body = body;
			diagnostics.ActionName = type.DisplayName();
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			body = diagnostics.ToString(db);
			if (isHtml)
				body = "<p style='white-space: pre-wrap;'>" + body + "</p>";
			SendEmail(mailInfo, subject, body, attachments, isHtml);

			diagnostics = new DiagnosticInformation(currentUser) {
				IP = IP
			};
			attachments = new List<string>();
			isHtml = false;
		}

		public void NewsChanged(News news, string description, string root)
		{
			var newsSubject = $"Оглавление: {news.Subject}";
			var newsBody = $"Текст: {news.Body}";
			var values = db2.Entry(news).OriginalValues;
			if (!Equals(values["Subject"], news.Subject)) {
				newsSubject = $"Изменилось оглавление<br>Было: {values["Subject"]}<br>Стало:{news.Subject}";
			}
			if (!Equals(values["Body"], news.Body)) {
				newsBody = $"Изменился текст<br>Было: {values["Body"]}<br>Стало:{news.Body}";
			}
			var body = $@"
<head>
<base href=""{root}"" target=""_blank"">
</head>
<body>
	Дата изменения: {DateTime.Now}<br>
	Сотрудник: {currentUser.Login} {currentUser.Name}<br>
	Хост: {currentUser.IP}<br>
	Операция: {description} <br/>
	{newsSubject}</br>
	{newsBody}</br>
</body>";
			SendEmail(ConfigurationManager.AppSettings["MailInfo"], description, body, new List<string>(), isHtml = true);
		}

		// Запрос регистрации производителя, расширенное сотрудникам
		public static void ProducerRequestMessage(producerinterface_Entities cntx, Account user, string message, string contacts)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ProducerRequest);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var body = $"{ReliableTokenizer(mailForm.Body, new { Message = message, Contacts = contacts })}";

			var di = new DiagnosticInformation(user) { Body = body, ActionName = MailType.ProducerRequest.DisplayName() };
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), null, false);
		}

		// Изменение ПКУ, сотрудникам
		public static void SendCatalogChangeMessage(producerinterface_Entities cntx, Account user, string fieldName, string formName, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.CatalogPKU);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var body = $"{ReliableTokenizer(mailForm.Body, new { FieldName = fieldName, FormName = formName, Before = before, After = after })}";

			var di = new DiagnosticInformation(user) { Body = body, ActionName = MailType.CatalogPKU.DisplayName() };
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Изменение описания препарата, сотрудникам
		public static void SendDescriptionChangeMessage(producerinterface_Entities cntx, Account user, string fieldName, string catalogName, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.CatalogDescription);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var body = $"{ReliableTokenizer(mailForm.Body, new { FieldName = fieldName, CatalogName = catalogName, Before = before, After = after })}";

			var di = new DiagnosticInformation(user) { Body = body, ActionName = MailType.CatalogDescription.DisplayName() };
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Именение МНН препарата, сотрудникам
		public static void SendMnnChangeMessage(producerinterface_Entities cntx, Account user, string catalogName, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.CatalogMNN);
			var subject = ReliableTokenizer(mailForm.Subject, new { SiteName = siteName });
			var body = $"{ReliableTokenizer(mailForm.Body, new { CatalogName = catalogName, Before = before, After = after })}";

			var di = new DiagnosticInformation(user) { Body = body, ActionName = MailType.CatalogMNN.DisplayName() };
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			EmailSender.SendEmail(catalogChangeEmail, subject, di.ToString(cntx), null, false);
		}

		// Отклонение правки в каталог
		public static void SendRejectCatalogChangeMessage(producerinterface_Entities cntx, Account user, string catalogName,
			string fieldName, string before, string after, string comment)
		{
			//throw new NotImplementedException();
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int) MailType.RejectCatalogChange);
			var subject = ReliableTokenizer(mailForm.Subject, new {SiteName = siteName});
			var header = ReliableTokenizer(mailForm.Header, new {UserName = user.Name});
			var body =
				$"{header}\r\n\r\n{ReliableTokenizer(mailForm.Body, new {FieldName = fieldName, CatalogName = catalogName, Before = before, After = after, Comment = comment})}\r\n\r\n{mailForm.Footer}";

			//var attachments = EmailSender.GetAttachments(cntx, MailType.RejectCatalogChange);
			EmailSender.SendEmail(user.Login, subject, body, new List<string>(), false);

			var di = new DiagnosticInformation(user) {Body = body, ActionName = MailType.RejectCatalogChange.DisplayName()};
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), new List<string>(), false);
		}

		// Обратная связь, сотрудникам
		public static void SendFeedBackMessage(producerinterface_Entities cntx, Account user, string message, string Ip)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			var subject = $"Сообщение пользователя с сайта {siteName}";

			var di = new DiagnosticInformation(user) { Body = message, ActionName = "Обратная связь" };
			EmailSender.SendEmail(mailInfo, subject, di.ToString(cntx), null, false);
		}

		public void PromotionNotification(MailType type, Promotion promotion, string comment = null)
		{
			var args = new {
				PromotionName = promotion.Name,
				Status = promotion.GetStatus().DisplayName(),
				Comment = comment,
				Http = "<a href='" + ConfigurationManager.AppSettings["SiteHttp"] + "/Promotion/Manage/" + promotion.Id + "'>ссылке</a>",
				UserName = currentUser.Name,
				UserLogin = currentUser.Login,
				Break = "<br />"
			};
			isHtml = true;
			MessageFromTemplate(type, promotion.Author.Login, args);
			if (promotion.GetStatus() == ActualPromotionStatus.NotConfirmed
				&& (type == MailType.CreatePromotion || type == MailType.EditPromotion)) {
				SendEmail(ConfigurationManager.AppSettings["MailInfo"], "Запрос на модерацию промоакции",
					$@"От пользователя {currentUser.Login} получен запрос на модерацию промоакции {promotion.Name}",
					new List<string>());
			}
		}

		private static List<string> GetAttachments(Context db2, MailType mailType)
		{
			var result = new List<string>();

			// U:\WebApps\ProducerInterface\bin\ -> U:\WebApps\ProducerInterface\var\
			var pathToBaseDir = ConfigurationManager.AppSettings["PathToBaseDir"];
			var baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToBaseDir));
			if (!Directory.Exists(baseDir))
				throw new NotSupportedException($"Не найдена директория {baseDir} для сохранения файлов");

			// общая директория медиафайлов
			var dir = Path.Combine(baseDir, "MediaFiles");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			// поддиректория для хранения файлов данного типа писем
			var subdir = Path.Combine(baseDir, "MediaFiles", mailType.ToString());
			if (!Directory.Exists(subdir))
				Directory.CreateDirectory(subdir);

			// записали файлы в директорию
			var email = db2.Emails.Find((int)mailType);
			foreach (var mediaFile in email.MediaFiles)
			{
				var file = new FileInfo($"{subdir}\\{mediaFile.ImageName}");
				if (!file.Exists)
					File.WriteAllBytes(file.FullName, mediaFile.ImageFile);
				result.Add(file.FullName);
			}
			return result;
		}

		public static string Render(string format, Dictionary<string, object> dictionary)
		{
			// получили лист всех имеющихся в формате подстановок
			IEnumerable<string> tokens;
			TokenStringFormat.TokenizeString(format, out tokens);
			var list = (List<string>)tokens;

			// все, что требуется, но не содержится в словаре
			var required = list.Where(x => !dictionary.ContainsKey(x)).Select(x => x).ToList();

			// добавили в словарь то, что требуется, но чего нет
			foreach (var r in required)
				dictionary.Add(r, null);

			return TokenStringFormat.Format(format, dictionary);
		}

		// исходное поведение: валится, если не приходит требующейся подстановки. Исправлено: просто не подставляется, но не валится
		private static string ReliableTokenizer(string format, object substitution)
		{
			var dictionary = ToMap(substitution);
			return Render(format, dictionary);
		}

		private static Dictionary<string, object> Merge(Dictionary<string, object> dst, Dictionary<string, object> src)
		{
			foreach (var pair in src) {
				dst[pair.Key] = pair.Value;
			}
			return dst;
		}

		public static Dictionary<string, object> ToMap(object substitution)
		{
			var dictionary = new Dictionary<string, object>();
			if (substitution != null) {
				foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(substitution))
					dictionary.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(substitution));
			}
			return dictionary;
		}

		private class DiagnosticInformation
		{
			public string Body { get; set; }
			public Account User { get; set; }
			public DateTime? ActionDate { get; set; }
			public string ActionName { get; set; }
			public long? ProducerId { get; set; }
			public string ProducerName { get; set; }
			public string ReportId { get; set; }
			public string ReportName { get; set; }
			public string ErrorMessage { get; set; }
			public string IP { get; set; }

			public DiagnosticInformation(Account user)
			{
				User = user;
			}

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
						ProducerName = "отсутствует";
				}

				if (!ActionDate.HasValue)
					ActionDate = DateTime.Now;

				sb.AppendLine(Body);
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Дополнительная информация");
				if (User != null)
				{
					sb.AppendLine($"пользователь: {User.Name} (id={User.Id}, {User.Login}, IP {User.IP})");
					sb.AppendLine($"производитель: {ProducerName} (id={ProducerId})");
				}
				else
					sb.AppendLine($"Незарегистрированный пользователь (IP {User?.IP ?? IP})");
				sb.AppendLine($"действие: {ActionName}, время {ActionDate}");
				if (!string.IsNullOrEmpty(ReportId))
					sb.AppendLine($"отчет: {ReportName} (id={ReportId})");
				if (!string.IsNullOrEmpty(ErrorMessage))
					sb.AppendLine($"сообщение об ошибке: {ErrorMessage}");
				if (User?.UserType == TypeUsers.ControlPanelUser)
					sb.AppendLine($"письмо отправлено из Панели управления администратором: {User.Login}");

				return sb.ToString();
			}
		}
	}
}