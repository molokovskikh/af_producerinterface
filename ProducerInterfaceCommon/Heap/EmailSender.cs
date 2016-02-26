using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.ContextModels;
using StringFormat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc.Html;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.Heap
{
	public class EmailSender
	{

		// https://msdn.microsoft.com/ru-ru/library/x5x13z6h(v=vs.110).aspx - async

		public static void SendEmail(List<string> to, string subject, string body, string path)
		{
			foreach (var s in to)
				SendEmail(s, subject, body, path);
		}

		public static void SendEmail(string to, string subject, string body, string path, bool HtmlBody = false)
		{
			var maFrom = new MailAddress(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["MailFromSubscription"], System.Text.Encoding.UTF8);
			var maTo = new MailAddress(to);

			using (var message = new MailMessage(maFrom, maTo))
			{
				message.Subject = subject;
				message.SubjectEncoding = System.Text.Encoding.UTF8;
				message.Body = body;
				message.BodyEncoding = System.Text.Encoding.UTF8;
				message.IsBodyHtml = false;

				if (HtmlBody)
				{
					message.IsBodyHtml = HtmlBody;
				}

				if (!String.IsNullOrEmpty(path))
				{
					var a = new Attachment(path);
					message.Attachments.Add(a);
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
		public static void AutoPostReportMessage(producerinterface_Entities cntx, long userId, jobextend jext, string path, List<string> mailTo)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var creator = cntx.Account.Single(x => x.Id == jext.CreatorId);
			var producerName = cntx.producernames.Single(x => x.ProducerId == jext.ProducerId).ProducerName;

			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.AutoPostReport);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });
			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { ReportName = jext.CustomName, CreatorName = creator.Name, ProducerName = producerName, DateTimeNow = DateTime.Now })}\r\n\r\n{mailForm.Footer}";
      EmailSender.SendEmail(mailTo, subject, body, path);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} (id={user.Id}, {user.Login}), изготовитель {producerName} (id={user.AccountCompany.ProducerId}), время {DateTime.Now}, отчет \"{jext.CustomName}\", задача {jext.JobName}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, bodyExtended, path);
		}

		// Ручная рассылка отчетов, пользователю и расширенное сотрудникам
		public static void ManualPostReportMessage(producerinterface_Entities cntx, long userId, jobextend jext, string path, List<string> mailTo)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var creator = cntx.Account.Single(x => x.Id == jext.CreatorId);
			var producerName = cntx.producernames.Single(x => x.ProducerId == jext.ProducerId).ProducerName;

			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ManualPostReport);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });
			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { ReportName = jext.CustomName, CreatorName = creator.Name, ProducerName = producerName, DateTimeNow = DateTime.Now, UserName = user.Name, UserLogin = user.Login })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(mailTo, subject, body, path);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} (id={user.Id}, {user.Login}), изготовитель {producerName} (id={user.AccountCompany.ProducerId}), время {DateTime.Now}, отчет \"{jext.CustomName}\", задача {jext.JobName}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, bodyExtended, path);
		}

		// Нет данных для формировании отчета, пользователю и расширенное сотрудникам
		public static void SendEmptyReportMessage(producerinterface_Entities cntx, long userId, string reportName, string jobName)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.EmptyReport);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });
			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { ReportName = reportName })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(user.Login, subject, body, null);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} (id={user.Id}, {user.Login}), изготовитель {GetCompanyname(user.Id, cntx)} (id={user.AccountCompany.ProducerId}), время {DateTime.Now}, отчет \"{reportName}\", задача {jobName}";
			var mailError = ConfigurationManager.AppSettings["MailError"];
			EmailSender.SendEmail(mailError, subject, bodyExtended, null);
		}

		// Ошибка при формировании отчета, пользователю и расширенное сотрудникам
		public static void SendReportErrorMessage(producerinterface_Entities cntx, long userId, string reportName, string jobName, string errorMessage)
		{
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)MailType.ReportError);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });
			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { ReportName = reportName })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(user.Login, subject, body, null);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} (id={user.Id}, {user.Login}), изготовитель {GetCompanyname(user.Id, cntx)} (id={user.AccountCompany.ProducerId}), время {DateTime.Now}, отчет \"{reportName}\", задача {jobName}, сообщение об ошибке {errorMessage}";
			var mailError = ConfigurationManager.AppSettings["MailError"];
			EmailSender.SendEmail(mailError, subject, bodyExtended, null);
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

		// Универсальное на смену пароля, пользователю и расширенное сотрудникам
		private static void SendPasswordMessage(producerinterface_Entities cntx, long userId, string password, MailType type, string ip)
		{
			// TODO при cron-запуске есть вероятность, что пользователя уже нет. Возможно, Главный пользователь Производителя
			var user = cntx.Account.Single(x => x.Id == userId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)type);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });
			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { Password = password })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(user.Login, subject, body, null, true);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} ({user.Login}), изготовитель {GetCompanyname(user.Id, cntx)}, время {DateTime.Now}, IP {ip}, действие {GetEnumDisplayName(type)}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, bodyExtended, null, false);
		}

		// Изменение описания препарата, сотрудникам
		public static void SendCatalogChangeMessage(producerinterface_Entities cntx, Account user, string field, long? id, string before, string after)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			var subject = $"Изменение описания препарата на сайте {siteName}";
			var body = $"Изменено поле {field} записи {id}\r\n\r\nБыло:\r\n\r\n{before}\r\n\r\nСтало:\r\n\r\n{after}";
			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} ({user.Login}), изготовитель {GetCompanyname(user.Id, cntx)}, время {DateTime.Now}, IP {user.IP}";
			EmailSender.SendEmail(catalogChangeEmail, subject, bodyExtended, null, false);
		}

		// Именение МНН препарата, сотрудникам
		public static void SendMnnChangeMessage(producerinterface_Entities cntx, Account user, string drugFamilyName, string mnnBefore, string mnnAfter)
		{
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var catalogChangeEmail = ConfigurationManager.AppSettings["CatalogChangeEmail"];
			var subject = $"Изменение МНН препарата на сайте {siteName}";
			var body = $"Изменен МНН препарата {drugFamilyName}\r\n\r\nБыло:\r\n\r\n{mnnBefore}\r\n\r\nСтало:\r\n\r\n{mnnAfter}";
			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {user.Name} ({user.Login}), изготовитель {GetCompanyname(user.Id, cntx)}, время {DateTime.Now}, IP {user.IP}";
			EmailSender.SendEmail(catalogChangeEmail, subject, bodyExtended, null, false);
		}

		// Создание акции, пользователю и расширенное сотрудникам
		public static void SendNewPromotion(producerinterface_Entities cntx, long userId, long PromotionId, string ip)
		{
			var User_ = cntx.Account.Where(x => x.Id == userId).First();
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var Promotion = cntx.promotions.Where(x => x.Id == PromotionId).First();
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];

			MailType Type = MailType.CreatePromotion;

			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)Type);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });

			//     Акция { PromotionName}
			//     изменена { UserName}. Посмотреть статус и изменить акцию { Http}

			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { PromotionName = Promotion.Name, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + Promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(User_.Login, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", null, true);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {User_.Name} ({User_.Login}), изготовитель {GetCompanyname(User_.Id, cntx)}, время {DateTime.Now}, IP {ip}, действие {GetEnumDisplayName(Type)}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + bodyExtended + "</p>", null, true);
		}

		// Изменение акции, пользователю и расширенное сотрудникам
		public static void SendChangePromotion(producerinterface_Entities cntx, long userId, long PromotionId, string ip)
		{
			var User_ = cntx.Account.Where(x => x.Id == userId).First();

			long UserCreatePromotionID = cntx.promotions.Where(xxx => xxx.Id == PromotionId).First().ProducerUserId;
			var SendEmailOnCreateUserPromotion = cntx.Account.Where(xxx => xxx.Id == UserCreatePromotionID).First();

			var EmailCreateUser = cntx.promotions.Where(xxx => xxx.Id == PromotionId).Select(yyy => yyy.ProducerUserId);
			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var Promotion = cntx.promotions.Where(x => x.Id == PromotionId).First();
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];

			MailType Type = MailType.EditPromotion;

			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)Type);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });

			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { PromotionName = Promotion.Name, UserName = SendEmailOnCreateUserPromotion.Name, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + Promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(SendEmailOnCreateUserPromotion.Login, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", null, true);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {User_.Name} ({User_.Login}), изготовитель {GetCompanyname(User_.Id, cntx)}, время {DateTime.Now}, IP {ip}, действие {GetEnumDisplayName(Type)}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + bodyExtended + "</p>", null, true);
		}

		// Подтверждение акции, пользователю и расширенное сотрудникам
		public static void SendPromotionStatus(producerinterface_Entities cntx, long userId, long PromotionId, string ip)
		{
			var User_ = cntx.Account.Where(x => x.Id == userId).First();

			var siteName = ConfigurationManager.AppSettings["SiteName"];
			var Promotion = cntx.promotions.Where(x => x.Id == PromotionId).First();
			var siteHttp = ConfigurationManager.AppSettings["SiteHttp"];
			var SendEmailOnCreateUserPromotion = cntx.Account.Where(xxx => xxx.CompanyId == cntx.promotions.Where(yyy => yyy.Id == PromotionId).First().ProducerUserId).Select(zzz => zzz.Login).First();

			MailType Type = MailType.StatusPromotion;
			var mailForm = cntx.mailformwithfooter.Single(x => x.Id == (int)Type);
			var subject = TokenStringFormat.Format(mailForm.Subject, new { SiteName = siteName });

			string StatusPromotion = "Деактивирована";
			if (Promotion.Status) { StatusPromotion = "Подтверждена"; }


			var body = $"{mailForm.Header}\r\n\r\n{TokenStringFormat.Format(mailForm.Body, new { PromotionName = Promotion.Name, Status = StatusPromotion, Http = "<a href='" + siteHttp + "/Promotion/Manage/" + Promotion.Id + "'>ссылке</a>" })}\r\n\r\n{mailForm.Footer}";
			EmailSender.SendEmail(SendEmailOnCreateUserPromotion, subject, "<p style='white-space: pre-wrap;'>" + body + "</p>", null, true);

			var bodyExtended = $"{body}\r\n\r\nДополнительная информация:\r\nпользователь {User_.Name} ({User_.Login}), изготовитель {GetCompanyname(User_.Id, cntx)}, время {DateTime.Now}, IP {ip}, действие {GetEnumDisplayName(Type)}";
			var mailInfo = ConfigurationManager.AppSettings["MailInfo"];
			EmailSender.SendEmail(mailInfo, subject, "<p style='white-space: pre-wrap;'>" + bodyExtended + "</p>", null, true);

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

		public static string GetCompanyname(long UserId, producerinterface_Entities cntx)
		{
			var X = cntx.Account.Where(xxx => xxx.Id == UserId).First().AccountCompany;
			if (X.ProducerId == null || X.ProducerId == 0)
			{
				return X.Name;
			}
			else
			{
				return cntx.producernames.Where(xxx => xxx.ProducerId == X.ProducerId).First().ProducerName;
			}
		}
	}
}