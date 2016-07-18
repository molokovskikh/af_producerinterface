using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.Models;
using System.IO;
using ProducerInterfaceCommon.ContextModels;
using System.Configuration;
using System.Data;
using System.Linq;
using Quartz;
using System.Data.Entity;

namespace ProducerInterfaceCommon.Heap
{
	public class Processor<T> : IProcessor where T : ReportRow
	{
		private Type _type;

		private producerinterface_Entities db;

		private HeaderHelper _helper;

		public Processor()
		{
			_type = typeof(T);
      db = new producerinterface_Entities();
      _helper = new HeaderHelper();
    }

		public void Process(JobKey key, Report report, TriggerParam tparam)
		{
			// записали в историю запусков
			string mailTo = null;
			if (tparam.MailTo != null && tparam.MailTo.Count > 0)
				mailTo = string.Join(",", tparam.MailTo);
			var reportRunLog = new ReportRunLog() { JobName = key.Name, RunNow = false, MailTo = mailTo, AccountId = tparam.UserId };
			// записали IP только для ручного запуска
			var ip = "неизвестен (авт. запуск)";
			if (tparam is RunNowParam) {
				ip = ((RunNowParam)tparam).Ip;
				reportRunLog.Ip = ip;
				reportRunLog.RunNow = true;
			}
			db.ReportRunLog.Add(reportRunLog);
			db.SaveChanges();

			// вытащили расширенные параметры задачи
			var jext = db.jobextend.Single(x => x.JobName == key.Name
																						&& x.JobGroup == key.Group
																						&& x.Enable);
			// добавили сведения о последнем запуске
			jext.LastRun = DateTime.Now;
			db.SaveChanges();

			var querySort = report.Read<T>();
			var user = db.Account.First(x => x.Id == tparam.UserId);
			user.IP = ip;
			var mail = new EmailSender(db, new Context(), user);

			// действия при пустом отчете
			if (querySort.Count == 0) {
				jext.DisplayStatusEnum = DisplayStatus.Empty;
				db.Entry(jext).State = EntityState.Modified;
				db.SaveChanges();
				mail.SendEmptyReportMessage(jext);
				return;
			}

			var instance = (ReportRow)Activator.CreateInstance(_type);
			querySort = instance.Treatment<T>(querySort, report);

			var shredder = new ObjectShredder<T>();
			var dataTable = shredder.Shred(querySort);

			var headers = report.GetHeaders(_helper);

			var ds = CreateDataSet(report.CastomName, headers, dataTable);
			// записали XML
			var sw = new StringWriter();
			ds.WriteXml(sw, XmlWriteMode.WriteSchema);
			// сохранили в базу
			var jxml = db.reportxml.SingleOrDefault(x => x.JobName == key.Name && x.JobGroup == key.Group);
			if (jxml == null) {
				jxml = new reportxml() { JobName = key.Name, JobGroup = key.Group, SchedName = jext.SchedName, Xml = sw.ToString()};
				db.reportxml.Add(jxml);
			}
			else
				jxml.Xml = sw.ToString();

			// отправили статус, что отчет готов
			jext.DisplayStatusEnum = DisplayStatus.Ready;
			db.Entry(jext).State = EntityState.Modified;
			db.SaveChanges();

			// если указаны email - отправляем
			if (tparam.MailTo != null && tparam.MailTo.Count > 0) {
				// создали excel-файл
				var file = CreateExcel(key.Group, key.Name, ds, report);

				// при автоматическом и ручном запуске разное содержимое письма
				if (tparam is CronParam)
					mail.AutoPostReportMessage(jext, file.FullName, tparam.MailTo);
				else if (tparam is RunNowParam)
					mail.ManualPostReportMessage(jext, file.FullName, tparam.MailTo);
			}
		}

		public FileInfo CreateExcel(string jobGroup, string jobName, DataSet ds, Report param)
		{
			// U:\WebApps\ProducerInterface\bin\ -> U:\WebApps\ProducerInterface\var\
			var pathToBaseDir = ConfigurationManager.AppSettings["PathToBaseDir"];
			var baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToBaseDir));
			if (!Directory.Exists(baseDir))
				throw new NotSupportedException($"Не найдена директория {baseDir} для сохранения файлов");

			var dir = Path.Combine(baseDir, "Reports");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var subdir = Path.Combine(baseDir, "Reports", jobGroup);
			if (!Directory.Exists(subdir))
				Directory.CreateDirectory(subdir);

			var file = new FileInfo($"{subdir}\\{jobName}.xlsx");
			if (file.Exists)
				file.Delete();

			var data = ds.Tables["Data"];
			var headers = new List<string>();

			foreach (DataRow row in ds.Tables["Headers"].Rows)
				headers.Add(row[0].ToString());

			var title = ds.Tables["Titles"].Rows[0][0].ToString();

			// TODO именование листов. Сейчас лист называется именем, данным отчету пользователем
			var ecreator = new ExcelCreator<T>();
			ecreator.Create(file, title, headers, data, param);

			return file;
		}

		private DataSet CreateDataSet(string title, List<string> headers, DataTable data)
		{
			// чтобы показать на экране
			var ds = new DataSet();
			// добавили название страницы
			var dtt = ds.Tables.Add("Titles");
			dtt.Columns.Add("Title", typeof(string));
			var tr = dtt.NewRow();
			tr["Title"] = title;
			dtt.Rows.Add(tr);
			// добавили заголовки
			var dth = ds.Tables.Add("Headers");
			dth.Columns.Add("Header", typeof(string));
			foreach (var header in headers)
			{
				var r = dth.NewRow();
				r["Header"] = header;
				dth.Rows.Add(r);
			}
			// добавили данные
			data.TableName = "Data";
			ds.Tables.Add(data);
			return ds;
		}
	}
}