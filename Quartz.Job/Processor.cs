using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Quartz.Job.Models;
using System.IO;
using Quartz.Job.EDM;
using System.Configuration;
using System.Data;
using System.Linq;

namespace Quartz.Job
{
	public class Processor<T> where T : ReportRow
	{
		private Type _type;

		private reportData _cntx;

		private HeaderHelper _helper;

		public Processor()
		{
			_type = typeof(T);
			_cntx = new reportData();
			_helper = new HeaderHelper(_cntx);
		}

		public void Process(JobKey key, Report jparam, TriggerParam tparam)
		{
			// вытащили расширенные параметры задачи
			var jext = _cntx.jobextend.Single(x => x.JobName == key.Name
																							&& x.JobGroup == key.Group
																							&& x.Enable == true);

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

			// действия при пустом отчёте
			if (querySort.Count == 0) {
				jext.DisplayStatusEnum = DisplayStatus.Empty;
				_cntx.SaveChanges();
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

			// если запущено вручную с указанием показать на экране
			//if (tparam is RunNowParam && ((RunNowParam)tparam).ByDisplay)

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

			// отправили статус, что отчёт готов
			jext.DisplayStatusEnum = DisplayStatus.Ready;
			_cntx.SaveChanges();

			// если запущено кроном или запущено вручную с указанием отправить на почту
			if (tparam is CronParam || (tparam is RunNowParam && ((RunNowParam)tparam).ByEmail)) {
				var file = new FileInfo($"{subdir}\\{key.Name}.xlsx");
				if (file.Exists)
					file.Delete();

				// TODO именование листов. Сейчас лист называется именем, данным отчёту пользователем
				var ecreator = new ExcelCreator<T>();
				ecreator.Create(file, jparam.CastomName, headers, dataTable);

				// TODO тема и текст письма
				EmailSender.SendEmail(jparam.MailTo, jparam.MailSubject, jparam.CastomName, file.FullName);
			}
		}
	}
}