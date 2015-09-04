using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Common.Schedule;
using Microsoft.Win32.TaskScheduler;
using System.Configuration;
using AnalitFramefork.Components;

namespace Common.Schedule
{
	public class ScheduleHelper : IDisposable
	{
		public static string ScheduleServer = GlobalConfig.ScheduleServer;//ConfigurationManager.AppSettings["ScheduleServer"];
		public static string ScheduleDomainName = GlobalConfig.ScheduleDomainName; //ConfigurationManager.AppSettings["ScheduleDomainName"];
		public static string ScheduleUserName = GlobalConfig.ScheduleUserName; //ConfigurationManager.AppSettings["ScheduleUserName"];
		public static string SchedulePassword = GlobalConfig.SchedulePassword; //ConfigurationManager.AppSettings["SchedulePassword"];

		public static string ScheduleWorkDir = GlobalConfig.ScheduleWorkDir;// ConfigurationManager.AppSettings["ScheduleWorkDir"];
		public static string ScheduleAppPath = GlobalConfig.ScheduleAppPath; //ConfigurationManager.AppSettings["ScheduleAppPath"];
		public static string ReportsFolderName = GlobalConfig.ReportsFolderName; //ConfigurationManager.AppSettings["ReportsFolderName"];

		public TaskService service;
		public TaskFolder folder;

		public ScheduleHelper()
		{
			service = GetService();
			folder = GetReportsFolder(service);
		}

		public static TaskService GetService()
		{
#if TEST
			return new TaskService(ScheduleServer, ScheduleUserName, ScheduleDomainName, SchedulePassword);
#endif
#if DEBUG
			return new TaskService(null, null, null, null);
#endif
			return new TaskService(ScheduleServer, ScheduleUserName, ScheduleDomainName, SchedulePassword);
		}

		public static TaskFolder GetReportsFolder(TaskService taskService)
		{
			try
			{
				return taskService.GetFolder(ReportsFolderName);
			}
			catch (FileNotFoundException ex)
			{
				throw new Exception(String.Format("На сервере {0} не существует папка '{1}' в планировщике задач",
					ScheduleServer, ReportsFolderName), ex);
			}
		}

		public static void DeleteTask(TaskFolder reportsFolder, ulong generalReportId, string prefix)
		{
			try
			{
				reportsFolder.DeleteTask(prefix + generalReportId);
			}
			catch (FileNotFoundException)
			{
				//"Гасим" это исключение при попытке удалить задание, которого не существует
			}
		}

		public static Task CreateTask(TaskService taskService, TaskFolder reportsFolder, ulong generalReportId, string comment, string prefix)
		{
			var createTaskDefinition = taskService.NewTask();

			createTaskDefinition.RegistrationInfo.Author = ScheduleDomainName + "\\" + ScheduleUserName;
			createTaskDefinition.RegistrationInfo.Date = DateTime.Now;
			createTaskDefinition.RegistrationInfo.Description = comment;

			createTaskDefinition.Settings.AllowDemandStart = true;
			createTaskDefinition.Settings.AllowHardTerminate = true;
			createTaskDefinition.Settings.ExecutionTimeLimit = TimeSpan.FromHours(1);
			createTaskDefinition.Settings.RestartCount = 3;
			createTaskDefinition.Settings.RestartInterval = new TimeSpan(0, 15, 0);
			createTaskDefinition.Settings.StartWhenAvailable = true;
			var logonType = GetLogonType();
			//должны запускаться в не зависимости от того есть сессия пользователя или нет
			if (logonType == TaskLogonType.Password)
			{
				createTaskDefinition.Principal.LogonType = TaskLogonType.Password;
				createTaskDefinition.Principal.UserId = GetUser();
			}

			createTaskDefinition.Actions.Add(new ExecAction(ScheduleAppPath, "/gr:" + generalReportId, ScheduleWorkDir));

			return reportsFolder.RegisterTaskDefinition(
				prefix + generalReportId,
				createTaskDefinition,
				TaskCreation.Create,
				GetUser(),
				GetPassword(),
				logonType);
		}

		public static Task FindTask(TaskService taskService, TaskFolder reportsFolder, ulong generalReportId, string prefix)
		{
			return reportsFolder.Tasks.First(
				task => task.Name.Equals(prefix + generalReportId, StringComparison.OrdinalIgnoreCase));
		}

		public Task FindTask(ulong generalReportId, string prefix = "GR")
		{
			return FindTask(service, folder, generalReportId, prefix);
		}

		public Task FindTaskNullable(ulong generalReportId, string prefix = "GR")
		{
			return FindTaskNullable(folder, generalReportId, prefix);
		}

		public static Task FindTaskNullable(TaskFolder reportsFolder, ulong generalReportId, string prefix)
		{
			return reportsFolder.Tasks.FirstOrDefault(
				task => task.Name.Equals(prefix + generalReportId, StringComparison.OrdinalIgnoreCase));
		}

		public static Task UpdateTaskDefinition(TaskService taskService, TaskFolder reportsFolder, ulong generalReportId, TaskDefinition updateTaskDefinition, string prefix)
		{
			return reportsFolder.RegisterTaskDefinition(
				prefix + generalReportId,
				updateTaskDefinition,
				TaskCreation.Update,
				GetUser(),
				GetPassword(),
				GetLogonType());
		}

		public static TaskLogonType GetLogonType()
		{
			if (!String.IsNullOrEmpty(SchedulePassword))
				return TaskLogonType.Password;
			return TaskLogonType.InteractiveToken;
		}

		public static string GetPassword()
		{
			if (!String.IsNullOrEmpty(SchedulePassword))
				return SchedulePassword;
			return null;
		}

		public static string GetUser()
		{
			if (!String.IsNullOrEmpty(SchedulePassword))
				return ScheduleDomainName + "\\" + ScheduleUserName;
			return null;
		}

		public static IEnumerable<Task> GetAllTempTask(TaskFolder reportsFolder)
		{
			return reportsFolder.Tasks.Where(
				task => task.Name.IndexOf("temp", StringComparison.OrdinalIgnoreCase) != -1);
		}

		public Task GetTaskOrCreate(ulong id, string comment = "", string prefix = "GR")
		{
			return GetTaskOrCreate(service, folder, id, comment, prefix);
		}

		/// <summary>
		/// производим поиск задачи и обновление Description, если задача не существует, то она будет создана
		/// </summary>
		/// <param name="taskService"></param>
		/// <param name="reportsFolder"></param>
		/// <param name="generalReportId"></param>
		/// <param name="comment"></param>
		/// <returns></returns>
		public static Task GetTaskOrCreate(TaskService taskService, TaskFolder reportsFolder, ulong generalReportId, string comment, string prefix)
		{
			try
			{
				return FindTask(taskService, reportsFolder, generalReportId, prefix);
			}
			catch (InvalidOperationException)
			{
				return CreateTask(taskService, reportsFolder, generalReportId, comment, prefix);
			}
		}

		/// <summary>
		/// Выставляем состояние задачи (Включено / Выключено)
		/// </summary>
		/// <param name="reportId"></param>
		/// <param name="isEnable"></param>
		/// <param name="prefix"></param>
		public static void SetTaskEnableStatus(ulong reportId, bool isEnable, string prefix)
		{
			SetTaskInfo(reportId, prefix, definition => {
				definition.Settings.Enabled = isEnable;
			});
		}

		/// <summary>
		/// Обновляем комментарий задачи
		/// </summary>
		/// <param name="reportId"></param>
		/// <param name="comment"></param>
		/// <param name="prefix"></param>
		public static void SetTaskComment(ulong reportId, string comment, string prefix)
		{
			SetTaskInfo(reportId, prefix, definition => {
				definition.RegistrationInfo.Description = comment;
			});
		}

		/// <summary>
		/// Обновляет действия в задаче
		/// </summary>
		/// <param name="repoortId"></param>
		/// <param name="action"></param>
		/// <param name="prefix"></param>
		public static void SetTaskAction(ulong repoortId,
			string action,
			string prefix = "GR")
		{
			SetTaskInfo(repoortId,
				prefix,
				definition => {
					var newAction = new ExecAction(ScheduleAppPath, action, ScheduleWorkDir);
					definition.Actions.RemoveAt(0);
					definition.Actions.Add(newAction);
				});
		}

		public static void SetTaskInfo(ulong reportId,
			string prefix,
			Action<TaskDefinition> defitionAction)
		{
			TaskService service = GetService();
			TaskFolder folder = GetReportsFolder(service);
			Task task = FindTask(service, folder, reportId, prefix);
			if (task == null)
				return;

			TaskDefinition definition = task.Definition;

			defitionAction(definition);
			UpdateTaskDefinition(service, folder, reportId, definition, prefix);
		}

		public static void CreateFolderIfNeeded(TaskService taskService)
		{
			var root = taskService.RootFolder;
			var folder = root.SubFolders
				.FirstOrDefault(
				f => String.Equals(f.Name, ReportsFolderName, StringComparison.CurrentCultureIgnoreCase));
			if (folder == null)
				root.CreateFolder(ReportsFolderName);
		}

		public void Dispose()
		{
			if (service != null)
				service.Dispose();
			if (folder != null)
				folder.Dispose();
		}

		public void DeleteReportTask(ulong id)
		{
			DeleteTask(folder, id, "GR");
		}
	}
}