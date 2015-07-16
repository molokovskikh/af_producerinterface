using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using AnalitFramefork.Extensions;
using NHibernate;
using NHibernate.Context;

namespace AnalitFramefork
{
	/// <summary>
	/// Фреймворк для разработки приложений Аналит.
	/// </summary>
	public class Framework
	{
		protected static ISessionFactory SessionFactory;
		protected static ISession Session;
		protected static HttpApplication Application;
		protected static Assembly Assembly;

		public static void Initialize(HttpApplication application)
		{
			var type = application.GetType().BaseType;
			Assembly = Assembly.GetAssembly(type);
			Application = application;
			InitializeSessionFactory();
		}

		public static void InitializeSessionFactory()
		{
			var configuration = new NHibernate.Cfg.Configuration();
			var nhibernateConnectionString = ConfigurationManager.AppSettings["NhibernateConnectionString"];
			configuration.SetProperty("connection.provider", "NHibernate.Connection.DriverConnectionProvider")
				.SetProperty("connection.driver_class", "NHibernate.Driver.MySqlDataDriver")
				.SetProperty("connection.connection_string", nhibernateConnectionString)
				.SetProperty("dialect", "NHibernate.Dialect.MySQL5Dialect")
				.SetProperty("current_session_context_class", "web");

			//Раскоментировать, если необходимо изменять имена таблиц для моделей
			//configuration.SetNamingStrategy(new TableNamingStrategy());

			//Слушатели событий
			//configuration.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[] { new ModelCrudListener() };
			//configuration.EventListeners.PostInsertEventListeners = new IPostInsertEventListener[] { new ModelCrudListener() };
			//configuration.EventListeners.PreDeleteEventListeners = new IPreDeleteEventListener[] { new ModelCrudListener() };
			
			//Конфиги
			//var configurationPath = HttpContext.Current.Server.MapPath(@"~\Nhibernate\hibernate.cfg.xml");
			//configuration.Configure(configurationPath);

			//Раскоментировать для создания таблиц с помощью Nhibernate
			//var schema = new NHibernate.Tool.hbm2ddl.SchemaExport(configuration);
			//schema.Create(false, true);

			//Маппинг моделей при помощи аттрибутов
			var memstream = NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(Assembly);
			configuration.AddInputStream(memstream);

			//Создаем фабрику сессий
			SessionFactory = configuration.BuildSessionFactory();
		}

		public static ISession OpenSession()
		{
			if (!CurrentSessionContext.HasBind(SessionFactory))
			{
				Session = SessionFactory.OpenSession();
				CurrentSessionContext.Bind(Session);
				Session.BeginTransaction();
			}

			return Session;
		}

		public static void CloseSession(Exception exception = null)
		{
			Session.SafeTransactionCommit(exception);

			//Я не понимаю зачем нужна следующая команда
			Session = CurrentSessionContext.Unbind(SessionFactory);
			if (Session.IsOpen)
				Session.Close();
		}
	}
}