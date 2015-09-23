using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AnalitFramefork.Components;
using AnalitFramefork.Extensions;
using AnalitFramefork.Hibernate.Mapping.Attributes;
using AnalitFramefork.Hibernate.Models;
using MySql.Data.MySqlClient;
using NHibernate;
using ProducerControlPanel.Models;
using ProducerInterface.Models;

namespace ProducerInterfaceTest.Infrastructure.Helpers
{
	public class ModelCleaner
	{
		public void CleanDBs(ISession dbSession)
		{
			////Не проводить отчистку для баз данных:
			List<string> notCleanDbList = new List<string>()
			{
				"accessright",
				"ProducerInterface",
				"catalogs"
			};
			List<string> cleanDbList = new List<string>();
			List<Type> models = new List<Type>();
			// получение всех БД c моделями Solution(а)
			foreach (var a in AnalitFramefork.Framework.GetAssemblies()) {
				try {
					var tempList = Assembly.Load(a)
						.GetTypes()
						.Where(s => Attribute.IsDefined(s, typeof (ModelAttribute)))
						.ToList();
					models.AddRange(tempList);
				}
				catch (Exception ex) {
					// ignored
				}
			}
			foreach (var item in models) {
				BaseModel model = (BaseModel) Activator.CreateInstance(item);
				string st = model.GetDataBaseName();
				if (!cleanDbList.Any(s => s == st)) {
					cleanDbList.Add(st.ToLower());
				}
			}
			cleanDbList = cleanDbList.Where(s => !notCleanDbList.Any(d => d == s)).Select(s => s.ToLower()).ToList();

			//MySqlConnection
			var connectionString = AnalitFramefork.Framework.GetConnectionString();
			using (MySqlConnection connection = new MySqlConnection(connectionString)) {
				try {
					var cmd = new MySqlCommand();
					cmd.Connection = connection;
					cmd.Connection.Open();
					foreach (var dbname in cleanDbList) {
						cmd.CommandText = "SET foreign_key_checks = 0";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP DATABASE " + dbname;
						cmd.ExecuteNonQuery();
						cmd.CommandText = "SET foreign_key_checks = 1";
						cmd.ExecuteNonQuery();
						cmd.CommandText = string.Format("CREATE DATABASE `{0}`", dbname);
						cmd.ExecuteNonQuery();
					}
					cmd.Dispose();
				}
				finally {
					connection.Close();
				}
			}
			Console.WriteLine("Cleaned " + cleanDbList.Count + " databases");
		}

		public void RemoveModels(ISession dbSession)
		{
			var str = AnalitFramefork.Framework.GetConnectionString();
			if (string.IsNullOrEmpty(str) || str.Contains("analit.net"))
				throw new Exception("Нельзя проводить тесты на реальных базах данных analit.net");


			//Приоритет удаления данных (указываем только важные)
			List<Type> order = new List<Type>()
			{
				typeof (UserLogModel),
				typeof (AdminLogModel),
				typeof (UserPermission),
				typeof (ProfileNews),
				typeof (ProducerUser),
				typeof (Drug),
				typeof (DrugFamily),
				typeof (DrugForm),
				typeof (MNN),
				typeof (DrugDescription),
				typeof (DrugDescriptionRemark),
				typeof (Producer)
			};
			//Таблицы, которые не надо очищать (из полного списка моделей)
			List<Type> dontCleanList = new List<Type>()
			{
				typeof (MonthlySchedule),
				typeof (WeeklySchedule),
				typeof (SingleExecutionMailingAddress),
				typeof (ReportTypeProperty),
				typeof (ReportType),
				typeof (ReportSendLog),
				typeof (ReportPropertyValue),
				typeof (ReportPropertyListValue),
				typeof (ReportExecuteLog),
                typeof (ReportExecuteForm),
                typeof (Report),
                typeof (Payer),
                typeof (GeneralReport),
                typeof (Contact),
                typeof (AdminRole),
                typeof (AdminPermission),
                typeof (Admin)
				//no elements
				
			};
			////Обязательная отчистка - проводится в самом Начале
			List<string> tablesToCleanList_AtFirst = new List<string>()
			{
				"catalogs.products"
			};
			////Обязательная отчистка - проводится в самом конце
			List<string> tablesToCleanList_AtLast = new List<string>()
			{
				"accessright.AdminToAdminRole",
				"ProducerInterface.usertouserrole"
			};
			// получение всех типов моделей атрибутом Model
			foreach (var a in AnalitFramefork.Framework.GetAssemblies()) {
				try {
					var tempList = Assembly.Load(a)
						.GetTypes()
						.Where(s => Attribute.IsDefined(s, typeof (ModelAttribute)) && !order.Any(f => f == s))
						.ToList();
					order.AddRange(tempList);
				}
				catch (Exception ex) {
					// ignored
				}
			}
			//TODO: от чего-то тестируемая сборка не подхватывается (просмотреть, поправить)
			try {
				var tempList = Assembly.Load(Config.GetParam("ApplicationsToRun"))
					.GetTypes()
					.Where(s => Attribute.IsDefined(s, typeof (ModelAttribute)) && !order.Any(f => f == s))
					.ToList();
				order.AddRange(tempList);
			}
			catch (Exception ex) {
				// ignored
			}
			//убираем модели, что не нужно чистить
			dontCleanList.ForEach(s => order.Remove(s));

			foreach (var item in tablesToCleanList_AtFirst) {
				var query = "delete from " + item.ToLower() + "";
				dbSession.CreateSQLQuery(query).ExecuteUpdate();
				dbSession.Flush();
			}
			//отправляем запросы СУБД
			foreach (var item in order) {
				BaseModel model = (BaseModel) Activator.CreateInstance(item);
				var query = string.Format("delete from {0}.{1}", model.GetDataBaseName(),
					model.GetTableName());
				dbSession.CreateSQLQuery(query).ExecuteUpdate();
				dbSession.Flush();
			}
			foreach (var item in tablesToCleanList_AtLast) {
				var query = "delete from " + item.ToLower() + "";
				dbSession.CreateSQLQuery(query).ExecuteUpdate();
				dbSession.Flush();
			}
			Console.WriteLine("Cleaning " + order.Count + " tables");
		}
	}
}