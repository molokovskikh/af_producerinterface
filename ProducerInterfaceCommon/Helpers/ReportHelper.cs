using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using Quartz;
using Quartz.Impl;

namespace ProducerInterfaceCommon.Helpers
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

		public static string GetSchedulerName()
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
			// ����� ������
			var job = scheduler.GetJobDetail(key);

			var param = (Report)job.JobDataMap["param"];
			var jxml = db.reportxml.Single(x => x.JobName == jext.JobName);

			// �������� ����������� �����
			var ds = new DataSet();
			ds.ReadXml(new StringReader(jxml.Xml), XmlReadMode.ReadSchema);

			// ������� ��������� ��� ����� ���� �������
			var processor = param.GetProcessor();

			// ������� excel-����
			return processor.CreateExcel(jext.JobGroup, jext.JobName, ds, param);
		}

		/// <summary>
		/// ���������� ��� ������ �� �������������� ���� ������
		/// </summary>
		/// <param name="id">������������� ���� ������</param>
		/// <returns></returns>
		public Type GetModelType(int id)
		{
			var typeName = ((Reports)id).ToString();
			var type = Type.GetType($"ProducerInterfaceCommon.Models.{typeName}, {typeof(Report).Assembly.FullName}");
			if (type == null)
				throw new NotSupportedException($"�� ������� ������� ��� {typeName} �� �������������� {id}");
			return type;
		}

		/// <summary>
		/// ���������� ������ �� ��������� ������� (����������� ������������ � ������ �� ��� �� ������)
		/// </summary>
		/// <returns></returns>
		protected IScheduler GetDebagSheduler()
		{
			var props = (NameValueCollection)ConfigurationManager.GetSection("quartzDebug");
			var sf = new StdSchedulerFactory(props);
			var scheduler = sf.GetScheduler();

			// ��������� ��� ��������
			if (scheduler.SchedulerName != "TestScheduler")
				throw new NotSupportedException("������ �������������� TestScheduler");

			// ��������� ��������� �� �������
			var metaData = scheduler.GetMetaData();
			if (metaData.SchedulerRemote)
				throw new NotSupportedException("������ �������������� ��������� TestScheduler");

			if (!scheduler.IsStarted)
				scheduler.Start();
			return scheduler;
		}

		/// <summary>
		/// ���������� �������� ������� (���������������� �������� ��� win-������)
		/// </summary>
		/// <returns></returns>
		protected IScheduler GetRemoteSheduler()
		{
			var props = (NameValueCollection)ConfigurationManager.GetSection("quartzRemote");
			var sf = new StdSchedulerFactory(props);
			var scheduler = sf.GetScheduler();

			// ��������� ��� ��������
			if (scheduler.SchedulerName != "ServerScheduler")
				throw new NotSupportedException("������ �������������� ServerScheduler");

			// ��������� �������� �� �������
			var metaData = scheduler.GetMetaData();
			if (!metaData.SchedulerRemote)
				throw new NotSupportedException("������ �������������� ��������� ServerScheduler");

			return scheduler;
		}
	}
}
