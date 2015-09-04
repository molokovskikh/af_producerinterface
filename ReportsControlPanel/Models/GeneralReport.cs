using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using AnalitFramefork.Components;
using AnalitFramefork.Components.Validation;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using Common.Schedule;
using Microsoft.Win32.TaskScheduler;
using NHibernate;
using NHibernate.Collection;
using NHibernate.Linq;

namespace ReportsControlPanel.Models
{
	[Description("Генеральный отчет")]
	[Model(Database = "reports",Table = "general_reports")]
	public class GeneralReport : BaseModel
	{
		[Description("Номер"), Map(Column ="GeneralReportCode", PrimaryKey = true)]
		public virtual uint Id { get; set; }

		[Description("Название"), Map("EMailSubject"), ValidatorNotEmpty]
		public virtual string Title { get; set; }

		[Description("Плательщик"), BelongsTo("PayerID")]
		public virtual Payer Payer { get; set; }

		[Description("Включен"), Map("Allow")]
		public virtual bool Enabled { get; set; }

		[Map("ReportFileName")]
		public virtual string ReportFileName { get; set; }

		[Map("ReportArchName")]
		public virtual string ReportArchiveName { get; set; }

		[Description("Недельные расписания отчетов"), HasMany]
		public virtual IList<WeeklySchedule> WeeklySchedules { get; set; }

		[Description("Месячные расписания отчетов"), HasMany]
		public virtual IList<MonthlySchedule> MonthlySchedules { get; set; }

		[Description("Список запусков отчетов"), HasMany]
		public virtual IList<ReportExecuteLog> ReportExecuteLogs { get; set; }

		[Description("Список отсылок отчетов"), HasMany]
		public virtual IList<ReportSendLog> ReportSendLogs { get; set; }



		/// <summary>
		/// Подписчики отчета. Данная группа контактов может иметься и у других отчетов
		/// </summary>
		[Description("Группа подписчиков отчетов"), BelongsTo(Column = "ContactGroupId")]
		public virtual ContactGroup SharedContactGroup { get; set; }

		/// <summary>
		/// Собственные подписчики отчета. Данная группа контактов только к этому отчету
		/// </summary>
		[Description("Собственные подписчики отчета"), BelongsTo("PublicSubscriptionsId")]
		public virtual ContactGroup OwnContactGroup { get; set; }

		private Task _Task;
		private static ScheduleHelper _Helper;
		protected static ScheduleHelper Helper
		{
			get
			{
				if(_Helper == null)
					_Helper = new ScheduleHelper();
				return _Helper;;
			}
		}
		
		/// <summary>
		/// После загрузки расписаний, их необходимо синхронизировать.
		/// Так как модели в базе данных - это всего-лишь бутафория, когда реальные расписания находятся
		/// в планировщике Windows.
		/// </summary>
		/// <param name="session">Сессия БД Nhibernate</param>
		/// <param name="collection">Загруженная коллекция</param>
		public override void AfterCollectionLoad(ISession session, IPersistentCollection collection)
		{
			if(collection == WeeklySchedules)
				SyncSchedules(session,WeeklySchedules as IList, TaskTriggerType.Weekly);
			if (collection == MonthlySchedules)
				SyncSchedules(session, MonthlySchedules as IList, TaskTriggerType.Monthly);
		}

		private void SyncSchedules(ISession session, IList collection, TaskTriggerType triggerType)
		{
			var list = new List<Schedule>();
			var task = GetTask();
			for(var i = 0; i < task.Definition.Triggers.Count; i++)
			{
				if(task.Definition.Triggers[i].TriggerType != triggerType)
					continue;
				var schedule = GetScheduleForTrigger(i,collection, session);
				schedule.CopyPropertiesFromTrigger(task.Definition.Triggers[i]);
				schedule.GeneralReport = this;
				list.Add(schedule);
			}

			var orphans = collection.Cast<Schedule>().Where(i => !list.Contains(i)).ToList();
			foreach (var item in orphans)
			{
				collection.Remove(item);
			}
			session.Save(this);
			session.Flush();
		}


		/// <summary>
		/// Получение  или создание модели недельного расписания отчетов для недельного триггера.
		/// </summary>
		/// <param name="trigger">Триггер планировщика Windows</param>
		/// <param name="collection"></param>
		/// <param name="session">Сессия БД Nhibernate</param>
		/// <returns></returns>
		private Schedule GetScheduleForTrigger(int index, IList collection, ISession session)
		{
			var task = GetTask();
			var trigger = task.Definition.Triggers[index];
            var id = trigger.Id;
			var scheduleType = collection.GetType().GetGenericArguments()[0];
			var castedList = collection.Cast <Schedule>();
			Schedule schedule = castedList.FirstOrDefault(i => i.Id.ToString() == id) ?? Activator.CreateInstance(scheduleType, new object[] {}) as Schedule;
			schedule.GeneralReport = this;
			if (schedule.IsNew())
			{
				schedule.NoSync = true;
				session.Save(schedule);
				session.Flush();
				collection.Add(schedule);
				//По непонятной причине триггеры не ведут себя как нормальные ссылки
				task.Definition.Triggers[index].Id = schedule.Id.ToString();
				ScheduleHelper.UpdateTaskDefinition(task.TaskService, task.Folder, Id, task.Definition, "GR");
			}
			return schedule;
		}

		/// <summary>
		/// Получение задачи текущего отчета из планировщика Windows
		/// </summary>
		/// <returns></returns>
		public virtual Task GetTask()
		{
			if (_Task != null)
				return _Task;
			var task = ScheduleHelper.GetTaskOrCreate(Helper.service, Helper.folder, Id, "", "GR");
			_Task = task;
			return _Task;
		}

		/// <summary>
		/// Получение команды, которая выполняется при запуске отчета
		/// </summary>
		/// <returns></returns>
		public virtual string GetCommandText()
		{
			var action  =  GetTask().Definition.Actions.FirstOrDefault() as ExecAction;
			if (action == null)
				return null;
			var cmd = action.Path + " " + action.Arguments;
            return cmd;
		}

		/// <summary>
		/// Получение пути к папке планировщика Windows
		/// </summary>
		/// <returns></returns>
		public virtual string GetFolderPath()
		{
			var action = GetTask().Definition.Actions.FirstOrDefault() as ExecAction;
			if (action == null)
				return null;
			var path = action.WorkingDirectory;
            return path;
		}

		/// <summary>
		/// Отправка готового отчета
		/// </summary>
		/// <param name="session">Сессия Nhibernate</param>
		/// <param name="mails">Список адресов эл. почты на который надо отправить очет</param>
		public virtual void ResendReport(ISession session, List<string> mails)
		{
			var log = session.Query<ReportExecuteLog>()
				.Where(l => l.IsFailed != false && l.EndTime != null && l.GeneralReport == this)
				.OrderByDescending(l => l.Id)
				.Take(1)
				.FirstOrDefault();
			var files = new string[0];
			if (log != null)
				files = Directory.GetFiles(GlobalConfig.GetParam("ReportHistoryPath"), log.Id + ".*");
			if (files.Length == 0)
			{
				AddError("Файл отчета не найден");
				return;
			}

			var message = new MailMessage();
			message.From = new MailAddress("report@analit.net", "АК Инфорум");
			foreach (var mail in mails)
				message.To.Add(mail);
			
			message.Subject = Title;
			message.Attachments.Add(new Attachment(files[0])
			{
				Name = GetFilename(files[0])
			});

			var client = new SmtpClient();
			message.BodyEncoding = System.Text.Encoding.UTF8;
			client.Send(message);
		}

		/// <summary>
		/// Получение названия файла отчета
		/// </summary>
		/// <returns></returns>
		public virtual string GetReportFileName()
		{
				if (!String.IsNullOrEmpty(ReportFileName))
					return Path.ChangeExtension(ReportFileName, ".xls");
				return String.Format("Rep{0}.xls", Id);
		}

		/// <summary>
		/// Полученгие названия архива
		/// </summary>
		/// <returns></returns>
		public virtual string GetReportArchiveName()
		{
				if (!String.IsNullOrEmpty(ReportArchiveName))
					return ReportArchiveName;
				return Path.ChangeExtension(GetReportFileName(), ".zip");
		}

		/// <summary>
		/// Получение имени файла отчета из пути к файлу
		/// </summary>
		/// <param name="name">Путь к файлу</param>
		/// <returns></returns>
		public virtual string GetFilename(string name)
		{
			if (String.Equals(Path.GetExtension(name), ".zip", StringComparison.InvariantCultureIgnoreCase))
				return GetReportArchiveName();
			
			return GetReportFileName();
		}

		/// <summary>
		/// Выполнение отчета
		/// </summary>
		/// <param name="from">Дата от которой производится обработка</param>
		/// <param name="to">Дата по которую производится обработка</param>
		/// <param name="emails">Список адресов, куда отправить очет</param>
		/// <param name="session">Сессия базы данных</param>
		/// <param name="comment">Комментарий к запуску (отображается в экшенах планировщика Windows)</param>
		public virtual void Execute(DateTime from, DateTime to, List<string> emails, ISession session, string comment)
		{
			AddTempEmails(emails, session);
			var thisTask = ScheduleHelper.GetTaskOrCreate(GetTask().Folder.TaskService, GetTask().Folder, Convert.ToUInt64(Id), comment, "temp_");
			var newAction = new ExecAction(ScheduleHelper.ScheduleAppPath,"/gr:" + Id +
					string.Format(" /inter:true /dtFrom:{0} /dtTo:{1} /manual:true", from.ToShortDateString(), to.ToShortDateString()),
				ScheduleHelper.ScheduleWorkDir);

			var taskDefinition = thisTask.Definition;
			taskDefinition.Actions.RemoveAt(0);
			taskDefinition.Actions.Add(newAction);
			taskDefinition.RegistrationInfo.Description = comment;
			taskDefinition.Settings.RunOnlyIfIdle = false;
			taskDefinition.Settings.StopIfGoingOnBatteries = false;
			taskDefinition.Settings.DisallowStartIfOnBatteries = false;
			taskDefinition.Settings.StopIfGoingOnBatteries = false;
			ScheduleHelper.UpdateTaskDefinition(GetTask().Folder.TaskService, GetTask().Folder, Convert.ToUInt64(Id), taskDefinition, "temp_");
			if (thisTask.State != TaskState.Running)
			{
				thisTask.Run();
			}
			else
			{
				AddError("Отчет уже запущен");
			}
		}

		public virtual TaskState GetState()
		{
			var task = GetTask();
			return task.State;
		}

		public virtual string GetStateDescription()
		{
			var state = GetState();
			switch (state)
			{
				case TaskState.Disabled: return "Отключен";
				case TaskState.Queued: return "В очереди";
				case TaskState.Ready: return "Готов";
				case TaskState.Running: return "Запущен";
				default: return "Неизвестно";
			}
		}
		/// <summary>
		/// Добавление временных адресов, на который будет выслан отчет
		/// </summary>
		/// <param name="emails">Список адресов эл. почты</param>
		/// <param name="session">Сессия Nhibernate</param>
		protected virtual void AddTempEmails(List<string> emails, ISession session)
		{
			foreach (var email in emails)
			{
				var obj = new SingleExecutionMailingAddress();
				obj.Email = email;
				obj.GeneralReport = this;
				session.Save(obj);
			}
			session.Flush();
		}
	}
}