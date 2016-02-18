﻿using System;
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
	public class Processor<T> where T : ReportRow
	{
		private Type _type;

		private ProducerInterfaceCommon.ContextModels.producerinterface_Entities _cntx;

		private HeaderHelper _helper;

		public Processor()
		{
			_type = typeof(T);
      _cntx = new ProducerInterfaceCommon.ContextModels.producerinterface_Entities();
      _helper = new HeaderHelper(_cntx);
		}

		public void Process(JobKey key, Report jparam, TriggerParam tparam)
		{
			// записали в историю запусков только для крона. При запуске вручную - пишем на странице запуска, потому что знаем пользователя
			if (tparam is CronParam) {
				var reportRunLog = new ReportRunLog() { JobName = key.Name, RunNow = false };
				_cntx.ReportRunLog.Add(reportRunLog);
				_cntx.SaveChanges();
			}

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
						var mapper = new AutoMapper<T>();
						querySort = mapper.Map(reader);
					}
				}
			}

			// действия при пустом отчете
			if (querySort.Count == 0) {
				jext.DisplayStatusEnum = DisplayStatus.Empty;
				_cntx.SaveChanges();
				EmailSender.SendEmptyReportMessage(_cntx, tparam.UserId, jparam.CastomName, key.Name);
				return;
			}

			var instance = (ReportRow)Activator.CreateInstance(_type);
			querySort = instance.Treatment<T>(querySort);

			var shredder = new ObjectShredder<T>();
			var dataTable = shredder.Shred(querySort);

			var headers = jparam.GetHeaders(_helper);

			var dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Reports");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var subdir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Reports", key.Group);
			if (!Directory.Exists(subdir))
				Directory.CreateDirectory(subdir);

			// чтобы показать на экране
			var ds = new DataSet();
			// добавили название страницы
			var dtt = ds.Tables.Add("Titles");
			dtt.Columns.Add("Title", typeof(string));
			var tr = dtt.NewRow();
			tr["Title"] = jparam.CastomName;
			dtt.Rows.Add(tr);
			// добавили заголовки
			var dth = ds.Tables.Add("Headers");
			dth.Columns.Add("Header", typeof(string));
			foreach (var header in headers) {
				var r = dth.NewRow();
				r["Header"] = header;
				dth.Rows.Add(r);
			}
			// добавили данные
			dataTable.TableName = "Data";
			ds.Tables.Add(dataTable);
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
				var file = new FileInfo($"{subdir}\\{key.Name}.xlsx");
				if (file.Exists)
					file.Delete();

				// TODO именование листов. Сейчас лист называется именем, данным отчету пользователем
				var ecreator = new ExcelCreator<T>();
				ecreator.Create(file, jparam.CastomName, headers, dataTable);

				// при автоматическом и ручном запуске разное содержимое письма
				if (tparam is CronParam)
					EmailSender.AutoPostReportMessage(_cntx, tparam.UserId, jext, file.FullName, tparam.MailTo);
				else if (tparam is RunNowParam)
					EmailSender.ManualPostReportMessage(_cntx, tparam.UserId, jext, file.FullName, tparam.MailTo);
			}
		}
	}
}