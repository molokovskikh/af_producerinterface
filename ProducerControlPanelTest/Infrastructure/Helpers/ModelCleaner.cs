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
using ProducerInterface.Models;

namespace ProducerControlPanelTest.Infrastructure.Helpers
{
	public class ModelCleaner: ProducerInterfaceTest.Infrastructure.Helpers.ModelCleaner
	{  
		public new void RemoveModels(ISession dbSession)
		{
			base.RemoveModels(dbSession);
			var str = AnalitFramefork.Framework.GetConnectionString();
			if (string.IsNullOrEmpty(str) || str.Contains("analit.net"))
				throw new Exception("Нельзя проводить тесты на реальных базах данных analit.net");
			
			//Приоритет удаления данных (указываем только важные)
			List<Type> order = new List<Type>()
			{
				typeof (Admin),
                typeof (Producer)
			};
			//Таблицы, которые не надо очищать (из полного списка моделей)
			List<Type> dontCleanList = new List<Type>()
			{
				//no elements
			};
			////Обязательная отчистка - проводится в самом Начале
			List<string> tablesToCleanList_AtFirst = new List<string>()
			{
				//no elements
			};
			////Обязательная отчистка - проводится в самом конце
			List<string> tablesToCleanList_AtLast = new List<string>()
			{
				//no elements
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