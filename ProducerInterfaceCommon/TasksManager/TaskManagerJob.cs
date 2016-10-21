using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NHibernate;
using NHibernate.Linq;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceCommon.TasksManager.Services;
using Quartz;

namespace ProducerInterfaceCommon.TasksManager
{
	public class TaskManagerJob : IJob
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (TaskManagerJob));
		public static ISessionFactory DbFactory { get; set; }


		public void Execute(IJobExecutionContext context)
		{
			if (DbFactory == null) {
				var nh = new ProducerInterfaceCommon.ContextModels.NHibernate();
				nh.Init();
				DbFactory = nh.Factory;
			}

			var dbSession = DbFactory.OpenSession();
			try {
				var serviceTaskManager =
					dbSession.Query<ServiceTaskManager>().FirstOrDefault(s => s.JobName == context.JobDetail.Key.Name);
				if (serviceTaskManager == null) {
					logger.Error($"Сервис с ключем {context.JobDetail.Key.Name} не найден. Запуск данного сервиса остановлен.");

					//нужно уточнить, что делать с задачей, если ее нет в БД, останавливать или добалвять в БД
					var tManager = new TaskManager();
					tManager.ServiceQuartzStop(context.JobDetail.Key.Name);
				}

				var type = Type.GetType($"{serviceTaskManager.ServiceType}, {typeof (Report).Assembly.FullName}");
				var service = (BaseTaskManagerService) Activator.CreateInstance(type);
				service.ServiceRun(dbSession);

				serviceTaskManager.LastRun = SystemTime.Now().DateTime;
				dbSession.Save(serviceTaskManager);
			} finally {
				dbSession.Flush();
				dbSession.Close();
			}
		}
	}
}