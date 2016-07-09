using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace ProducerInterfaceCommon.Controllers
{
	public class ReportHelper
	{
		private producerinterface_Entities db;

		public ReportHelper(producerinterface_Entities db)
		{
			this.db = db;
		}

		public IScheduler GetScheduler()
		{
#if DEBUG
			return GetDebagSheduler();
#else
			return GetRemoteSheduler();
#endif
		}

		public string GetSchedulerName()
		{
#if DEBUG
			return "TestScheduler";
#else
			return "ServerScheduler";
#endif
		}

		public FileInfo GetExcel(jobextend jext)
		{

			var key = JobKey.Create(jext.JobName, jext.JobGroup);
			var scheduler = GetScheduler();
			// нашли задачу
			var job = scheduler.GetJobDetail(key);

			var param = (Report)job.JobDataMap["param"];
			var jxml = db.reportxml.Single(x => x.JobName == jext.JobName);

			// вытащили сохраненный отчет
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			// создали процессор для этого типа отчетов
			var processor = param.GetProcessor();

			// создали excel-файл
			return processor.CreateExcel(jext.JobGroup, jext.JobName, ds, param);
		}

		/// <summary>
		/// Возвращает тип отчета по идентификатору типа отчета
		/// </summary>
		/// <param name="id">Идентификатор типа отчета</param>
		/// <returns></returns>
		public Type GetModelType(int id)
		{
			var typeName = ((Reports)id).ToString();
			var type = Type.GetType($"ProducerInterfaceCommon.Models.{typeName}, {typeof(Report).Assembly.FullName}");
			if (type == null)
				throw new NotSupportedException($"Не удалось создать тип {typeName} по идентификатору {id}");
			return type;
		}

		/// <summary>
		/// Возвращает ссылку на локальный шедулер (запускаемый одновременно с сайтом на той же машине)
		/// </summary>
		/// <returns></returns>
		protected IScheduler GetDebagSheduler()
		{
			var props = (NameValueCollection)ConfigurationManager.GetSection("quartzDebug");
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
		protected IScheduler GetRemoteSheduler()
		{
			var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
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
	}
}
