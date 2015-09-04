using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AnalitFramefork.Components.Models;
using AnalitFramefork.Components.Validation;
using NHibernate;
using NHibernate.Validator.Engine;

namespace ReportsControlPanel.Models
{
	/// <summary>
	/// Модель для формы выполнения / отсылки отчета
	/// </summary>
	public class ReportExecuteForm : FormModel
	{
		[ValidatorNotEmpty]
		public  DateTime? StartTime { get; set; }

		[ValidatorNotEmpty]
		public DateTime? EndTime { get; set; }

		[ValidatorNotNull]
		public string Emails { get; set; }

		[ValidatorNotNull]
		public string UserEmail { get; set; }

		[ValidatorNotNull]
		public bool? UseEmailList { get; set; }

		[ValidatorNotNull]
		public bool? Execute { get; set; }

		public override InvalidValue[] Validate(ISession session)
		{
			var list = new List<InvalidValue>();
			if (GetEmailList().Count <= 0)
				list.Add(new InvalidValue("Количество почтовых адресов должно быть больше 0",GetType(),"Emails",Emails,this,null));

			return list.ToArray();
		}

		/// <summary>
		/// Получение списка адресов электронной потчы
		/// </summary>
		/// <returns></returns>
		private List<string> GetEmailList()
		{
			var list = new List<string>();
			if (UseEmailList.HasValue && UseEmailList.Value != true)
				list.Add(UserEmail);
			else
				list = Emails.Trim().Split(',').ToList();

			for(var i =0; i < list.Count; i++)
				list[i] = list[i].Trim(new[] { ' ', '\n', '\r' });
			return list;
		}

		/// <summary>
		/// Обработка отчета формой
		/// </summary>
		/// <param name="report">Отчет</param>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="comment">Комментарий к запуску</param>
		public void ProcessReport(GeneralReport report, ISession session, string comment = "")
		{
			if (Execute.HasValue && Execute.Value == true)
				ExecuteReport(report, session, comment);
			else
				SendReport(report,session);
		}

		/// <summary>
		/// Единоразовый запуск отчета
		/// </summary>
		/// <param name="report">Отчет</param>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="comment">Комментарий к запуску</param>
		private void ExecuteReport(GeneralReport report, ISession session, string comment = "")
		{
			report.Execute(StartTime.Value ,EndTime.Value, GetEmailList(), session, comment);
			LastActionErrors = report.GetErrors();
		}

		/// <summary>
		/// Отсылка сохраненной копии отчета
		/// </summary>
		/// <param name="report">Отчет</param>
		/// <param name="session">Сессия Nhibernate</param>
		private void SendReport(GeneralReport report, ISession session)
		{
			var mails = GetEmailList();
			report.ResendReport(session, mails);
			LastActionErrors = report.GetErrors();
		}

		/// <summary>
		/// Импортирование списка имейлов для отправки одноразового отчета из подписок отчета.
		/// </summary>
		/// <param name="report">Отчет</param>
		public void ImportEmailsFromReport(GeneralReport report)
		{
			var list = report.OwnContactGroup.Contacts.ToList().Select(i => i.ContactText).ToList();
			list.AddRange(report.SharedContactGroup.Contacts.Select(i => i.ContactText).ToList());
			Emails = string.Join(",\n", list);
		}
	}
}