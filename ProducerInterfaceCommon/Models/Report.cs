using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using Quartz;

namespace ProducerInterfaceCommon.Models
{
	[Serializable]
	public abstract class Report
	{
		public abstract string Name { get; }

		[Display(Name = "Название отчета")]
		[Required(ErrorMessage = "Не указано название отчета")]
		[StringLength(250, ErrorMessage = "Название отчета должно быть не длиннее 250 символов")]
		[UIHint("String")]
		public string CastomName { get; set; }

		// тип отчета по enum
		[HiddenInput(DisplayValue = false)]
		[Required]
		public int Id { get; set; }

		[ScaffoldColumn(false)]
		public long? ProducerId { get; set; }

		public abstract List<string> GetHeaders(HeaderHelper h);

		public virtual string GetSpName()
		{
			return null;
		}

		public virtual Dictionary<string, object> GetSpParams()
		{
			return new Dictionary<string, object>();
		}

		public virtual MySqlCommand GetCmd(MySqlConnection connection)
		{
			var command = new MySqlCommand(GetSpName(), connection);
			command.CommandType = CommandType.StoredProcedure;
			command.CommandTimeout = 0;
			foreach (var spparam in GetSpParams())
				command.Parameters.AddWithValue(spparam.Key, spparam.Value);
			return command;
		}

		public virtual List<ErrorMessage> Validate()
		{
			return new List<ErrorMessage>();
		}

		public abstract Dictionary<string, object> ViewDataValues(NamesHelper h);

		public abstract IProcessor GetProcessor();

		public List<T> Read<T>()
		{
			var connString = ConfigurationManager.ConnectionStrings["producerinterface"].ConnectionString;
			using (var conn = new MySqlConnection(connString)) {
				conn.Open();
				using (var command = GetCmd(conn)) {
					using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection)) {
						var mapper = new MyAutoMapper<T>();
						return mapper.Map(reader);
					}
				}
			}
		}

		public virtual void Init(Account currentUser)
		{
			ProducerId = currentUser.AccountCompany.ProducerId;
		}

		public void Run(JobKey key, TriggerParam interval)
		{
				var processor = GetProcessor();
				processor.Process(key, this, interval);
		}
	}
}