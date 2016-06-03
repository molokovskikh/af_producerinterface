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

namespace ProducerInterfaceCommon.Heap
{
	public class Processor<T> : IProcessor where T : ReportRow
	{
		private Type _type;

		private producerinterface_Entities _cntx;

		private HeaderHelper _helper;

		public Processor()
		{
			_type = typeof(T);
      _cntx = new producerinterface_Entities();
      _helper = new HeaderHelper(_cntx);
    }

		public void Process(JobKey key, Report jparam, TriggerParam tparam)
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
			_cntx.ReportRunLog.Add(reportRunLog);
			_cntx.SaveChanges();

			// вытащили расширенные параметры задачи
			var jext = _cntx.jobextend.Single(x => x.JobName == key.Name
																						&& x.JobGroup == key.Group
																						&& x.Enable == true);
			// добавили сведения о последнем запуске
			jext.LastRun = DateTime.Now;
			_cntx.SaveChanges();

			var querySort = new List<T>();
			var connString = ConfigurationManager.ConnectionStrings["producerinterface"].ConnectionString;
			using (var conn = new MySqlConnection(connString)) {
				using (var command = new MySqlCommand(jparam.GetSpName(), conn)) {
					command.CommandType = CommandType.StoredProcedure;
					command.CommandTimeout = 0;
					foreach (var spparam in jparam.GetSpParams())
						command.Parameters.AddWithValue(spparam.Key, spparam.Value);
					conn.Open();
					using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection)) {
						var mapper = new MyAutoMapper<T>();
						querySort = mapper.Map(reader);
					}
				}
			}

			// действия при пустом отчете
			if (querySort.Count == 0) {
				jext.DisplayStatusEnum = DisplayStatus.Empty;
				_cntx.SaveChanges();
				EmailSender.SendEmptyReportMessage(_cntx, tparam.UserId, jext, ip);
				return;
			}

			var instance = (ReportRow)Activator.CreateInstance(_type);
			querySort = instance.Treatment<T>(querySort, jparam);

			var shredder = new ObjectShredder<T>();
			var dataTable = shredder.Shred(querySort);

			var headers = jparam.GetHeaders(_helper);

			var ds = CreateDataSet(jparam.CastomName, headers, dataTable);
			// записали XML
			var sw = new StringWriter();
			ds.WriteXml(sw, XmlWriteMode.WriteSchema);
			// сохранили в базу
			var jxml = _cntx.reportxml.SingleOrDefault(x => x.JobName == key.Name && x.JobGroup == key.Group);
			if (jxml == null) {
				jxml = new reportxml() { JobName = key.Name, JobGroup = key.Group, SchedName = jext.SchedName, Xml = sw.ToString()};
				_cntx.reportxml.Add(jxml);
			}
			else 
				jxml.Xml = sw.ToString();

			// отправили статус, что отчет готов
			jext.DisplayStatusEnum = DisplayStatus.Ready;
			_cntx.SaveChanges();

			// если указаны email - отправляем
			if (tparam.MailTo != null && tparam.MailTo.Count > 0) {
				// создали excel-файл
				var file = CreateExcel(key.Group, key.Name, ds, jparam);

				// при автоматическом и ручном запуске разное содержимое письма
				if (tparam is CronParam)
					EmailSender.AutoPostReportMessage(_cntx, tparam.UserId, jext, file.FullName, tparam.MailTo, ip);
				else if (tparam is RunNowParam)
					EmailSender.ManualPostReportMessage(_cntx, tparam.UserId, jext, file.FullName, tparam.MailTo, ip);
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