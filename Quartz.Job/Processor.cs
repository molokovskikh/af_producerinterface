using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Quartz.Job.Models;
using System.IO;
using Quartz.Job.EDM;
using System.Configuration;
using System.Data;

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

		public void Process(Report param, JobKey key, bool runNow)
		{
			var querySort = new List<T>();
			var connString = ConfigurationManager.ConnectionStrings["quartz"].ConnectionString;
			using (var conn = new MySqlConnection(connString)) {
				using (var command = new MySqlCommand(param.GetSpName(), conn)) {
					command.CommandType = CommandType.StoredProcedure;
					foreach (var spparam in param.GetSpParams())
						command.Parameters.AddWithValue(spparam.Key, spparam.Value);
					conn.Open();
					using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection)) {
						var mapper = new AutoMapper<T>();
						querySort = mapper.Map(reader);
					}
				}
			}

			var instance = (ReportRow)Activator.CreateInstance(_type);
			querySort = instance.Treatment<T>(querySort);

			var shredder = new ObjectShredder<T>();
			var dataTable = shredder.Shred(querySort);

			var headers = param.GetHeaders(_helper);

			// TODO именование и хранение файлов
			var dir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Reports");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var file = new FileInfo($"{dir}\\{key.Name}.xlsx");
			if (file.Exists)
				file.Delete();

			// TODO именование листов. Сейчас лист называется именем, данным отчёту пользователем
			var ecreator = new ExcelCreator<T>();
			ecreator.Create(file, param.CastomName, headers, dataTable);

			// TODO тема и текст письма
			EmailSender.SendEmail(param.MailTo, param.MailSubject, param.CastomName, file.FullName);
		}
	}
}