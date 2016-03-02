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
using System.Web.Mvc;

namespace ProducerInterfaceCommon.Controllers
{
	public class BaseReportController : BaseController 
	{

		/// <summary>
		/// Возвращает отчет в виде excel-файла
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <returns></returns>
		public FileResult GetFile(string jobName)
		{
			var jext = cntx_.jobextend.Single(x => x.JobName == jobName);
			var file = GetExcel(jext);

			// вернули файл
			byte[] fileBytes = System.IO.File.ReadAllBytes(file.FullName);
			var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			return File(fileBytes, contentType, file.Name);
		}

		protected IScheduler GetScheduler()
		{
#if DEBUG
			return GetDebagSheduler();
#else
			return GetRemoteSheduler();
#endif
		}

		protected string GetSchedulerName()
		{
#if DEBUG
			return "TestScheduler";
#else
			return "ServerScheduler";
#endif
		}

		protected FileInfo GetExcel(jobextend jext)
		{
			var jxml = cntx_.reportxml.Single(x => x.JobName == jext.JobName);

			// вытащили сохраненный отчет
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			// создали процессор для этого типа отчетов
			var type = GetModelType(jext.ReportType);
			var report = (Report)Activator.CreateInstance(type);
			var processor = report.GetProcessor();

			// создали excel-файл
			return processor.CreateExcel(jext.JobGroup, jext.JobName, ds);
		}

		/// <summary>
		/// Возвращает тип отчета по идентификатору типа отчета
		/// </summary>
		/// <param name="id">Идентификатор типа отчета</param>
		/// <returns></returns>
		protected Type GetModelType(int id)
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
