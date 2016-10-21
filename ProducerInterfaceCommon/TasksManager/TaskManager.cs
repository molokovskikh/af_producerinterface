using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NHibernate;
using NHibernate.Linq;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Helpers;
using ProducerInterfaceCommon.Models;
using Quartz;
using Quartz.Impl;

namespace ProducerInterfaceCommon.TasksManager
{
	public class TaskManager
	{
		protected static readonly ILog logger = LogManager.GetLogger(typeof (TaskManager));

		public TaskManager()
		{
		}

		public IScheduler GetScheduler()
		{
#if DEBUG
			return GetDebugSheduler();
#else
			return GetReleaseSheduler();
#endif
		}

		/// <summary>
		/// Возвращает ссылку на локальный шедулер (запускаемый одновременно с сайтом на той же машине)
		/// </summary>
		/// <returns></returns>
		protected IScheduler GetDebugSheduler()
		{
			var props = (NameValueCollection) ConfigurationManager.GetSection("quartzDebug");
			var sf = new StdSchedulerFactory(props);
			var scheduler = sf.GetScheduler();

			// проверяем имя шедулера
			if (scheduler.SchedulerName != "TestScheduler")
				throw new NotSupportedException("Должен использоваться TestScheduler");

			// проверяем локальный ли шедулер
			var metaData = scheduler.GetMetaData();
			if (metaData.SchedulerRemote)
				throw new NotSupportedException("Должен использоваться локальный TestScheduler");

			if (!scheduler.IsStarted)
				scheduler.Start();
			return scheduler;
		}

		/// <summary>
		/// Возвращает удалённый шедулер (инсталлированный отдельно как win-служба)
		/// </summary>
		/// <returns></returns>
		protected IScheduler GetReleaseSheduler()
		{
			var props = (NameValueCollection) ConfigurationManager.GetSection("quartzRemote");
			var sf = new StdSchedulerFactory(props);
			var scheduler = sf.GetScheduler();

			// проверяем имя шедулера
			if (scheduler.SchedulerName != "ServerScheduler")
				throw new NotSupportedException("Должен использоваться ServerScheduler");

			// проверяем удалённый ли шедулер
			var metaData = scheduler.GetMetaData();
			if (!metaData.SchedulerRemote)
				throw new NotSupportedException("Должен использоваться удаленный ServerScheduler");

			return scheduler;
		}

		public static BaseTaskManagerService GetServiceByTypeName(string serviceTypeName)
		{
			var type = Type.GetType($"{serviceTypeName}, {typeof (Report).Assembly.FullName}");
			return (BaseTaskManagerService) Activator.CreateInstance(type);
		}

		public static void JobServiceUpdateServiceList(ISession dbSession)
		{
			var type = typeof (BaseTaskManagerService);
			var list = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => type.IsAssignableFrom(p) && p.IsAbstract == false);

			foreach (var item in list) {
				var itemInBase = dbSession.Query<ServiceTaskManager>().FirstOrDefault(s => s.ServiceType == item.FullName);
				if (itemInBase == null) {
					itemInBase = new ServiceTaskManager();
					itemInBase.JobName = Guid.NewGuid().ToString();
					itemInBase.ServiceName = item.Name;
					itemInBase.ServiceType = item.FullName;
					itemInBase.LastModified = SystemTime.Now().DateTime;
					itemInBase.CreationDate = SystemTime.Now().DateTime;
					dbSession.Save(itemInBase);
				}
			}
		}


		///  <summary>
		///	Начала выполнения задачи
		///  </summary>
		///  <param name="guid"></param>
		/// <param name="serviceType"></param>
		/// <param name="interval"></param>
		///  <returns></returns>
		public bool ServiceQuartzStart(string guid, string serviceType, string interval = "")
		{
			var result = false;
			guid = !string.IsNullOrEmpty(guid) ? guid : Guid.NewGuid().ToString();
			interval = !string.IsNullOrEmpty(interval) ? interval : "0 0 9 1 * ?"; // раз в месяц в 9 утра

			var tManager = new TaskManager();
			try {
				result = tManager.JobServiceStart(guid, serviceType, interval);
			} catch (Exception e) {
				logger.Error($"Ошибка при обновлении задачи '{guid}', '{serviceType}'", e);
			}
			return result;
		}

		/// <summary>
		/// Останавливает задачу
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public bool ServiceQuartzStop(string guid)
		{
			var scheduler = GetScheduler();
			var key = new JobKey(guid); // group можно привязать к типу сервиса, но название типа может изменится.
			var job = scheduler.GetJobDetail(key);
			if (job == null) {
				logger.Error($"Задача {guid} не найдена");
				return false;
			}
			try {
				scheduler.PauseJob(key);
			} catch (Exception e) {
				logger.Error($"Ошибка при остановке задачи {guid} для Scheduler", e);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Добавление / обновление задачи для Кварца
		/// </summary>
		/// <param name="createJobFunc"></param>
		/// <param name="guid"></param>
		/// <param name="desription"></param>
		/// <param name="cronExpressionInterval"></param>
		/// <returns>Успех действия</returns>
		private bool JobServiceStart(string guid, string desription, string cronExpressionInterval)
		{
			var scheduler = GetScheduler();

			var key = new JobKey(guid); // group можно привязать к типу сервиса, но название типа может изменится.

			var job = scheduler.GetJobDetail(key);
			if (job == null) {
				job = JobBuilder.Create<TaskManagerJob>()
					.WithIdentity(key)
					.StoreDurably()
					.WithDescription(desription)
					.Build();
			}
			try {
				scheduler.AddJob(job, true);
			} catch (Exception e) {
				logger.Error("Ошибка при добавлении задачи для Scheduler", e);
				return false;
			}

			//job.JobDataMap["param"] = model;

			CronParam triggerModel = new CronIntervalParam(); ///////Оставил, т.к. будет удобно крепить к UI
			// триггер вставлялся с идентификатором задачи
			var oldTriggerKey = new TriggerKey(key.Name);
			// используются только cron-триггеры
			var oldTrigger = (ICronTrigger) (scheduler.GetTrigger(oldTriggerKey));
			//// если триггер уже был - устанавливаем UI его значением
			if (oldTrigger != null)
				triggerModel = (CronParam) oldTrigger.JobDataMap["tparam"];

			//интервал
			triggerModel.CronExpression = cronExpressionInterval;
			//triggerModel.CronHumanText = "";


			// новый триггер для этой задачи
			var trigger = TriggerBuilder.Create()
				.WithIdentity(key.Name)
				.WithCronSchedule(triggerModel.CronExpression)
				.ForJob(job.Key)
				.WithDescription(triggerModel.CronHumanText)
				.Build();

			trigger.JobDataMap["tparam"] = triggerModel;

			try {
				if (oldTrigger == null)
					scheduler.ScheduleJob(trigger);
				else
					scheduler.RescheduleJob(oldTriggerKey, trigger);
			} catch (Exception e) {
				logger.Error($"Ошибка при добавлении расписания задачи {key.Name} для Scheduler", e);
				return false;
			}
			return true;
		}
	}
}