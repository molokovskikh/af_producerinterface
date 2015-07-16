﻿using System;
using System.Linq;
using System.Threading;
using AnalitFramefork.Components;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Mapping.Attributes;

namespace AnalitFramefork.Extensions
{
	/// <summary>
	/// Расширение класса сессии Nhibernate
	/// </summary>
	public static class SessionExtensions
	{
		private static int DeadlockWaitTime = 2000;
		private static int DeadlockWaitAttempts = 5;

		/// <summary>
		/// Пытается удалить объект. Возврает индикатор успешного выполнения задачи.
		/// Очищает сессию, в случае неудачи.
		/// </summary>
		/// <param name="DbSession">Сессия Nhibernate</param>
		/// <param name="model">Модель, которую необходимо удалить</param>
		/// <returns>True, в случае успеха</returns>
		public static bool AttemptDelete(this NHibernate.ISession DbSession, BaseModel model)
		{
			try {
				//Почему-то не инициализированные ManyToMany поля вызывают проблемы
				//todo проблема наблюдается в ModelCrudListener
				//Возможно в ModelCrudListener ее лучше лечить - пока мне кажется, что проще инициализировать поля, так как все-равно не будет особого ущерба производительности
				InitializeModel(model);
				DbSession.Delete(model);
				//Внешние ключи могут не дать удалить объект
				//В этом случае, если сейчас не закрыть транзакцию, то приложение упадет с ошибкой, когда будет когда будет
				//произведено автоматическое закрытие транзакии в BaseController
				DbSession.Transaction.Commit();
				return true;
			}
			catch (NHibernate.Exceptions.GenericADOException e)
			{
				DbSession.Clear();
			}
			return false;
		}

		/// <summary>
		/// Инициализвация полей модели. Пока только ManyToMany
		/// </summary>
		/// <param name="model">Модель, поля котоой необходимо инициализировать.</param>
		private static void InitializeModel(BaseModel model)
		{
			var props = model.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ManyToManyAttribute)));
			foreach (var prop in props) {
				var obj = prop.GetValue(model,new object[]{});
				NHibernateUtil.Initialize(obj);
			}
		}

		/// <summary>
		/// Безопасный коммит транзакции, лечит дедлоки
		/// </summary>
		/// <param name="DbSession">Сессия Nhibernate</param>
		/// <param name="exception">Ошибка в контроллере (обычно должен быть null)</param>
		/// <returns></returns>
		public static void SafeTransactionCommit(this ISession session, Exception exception = null)
		{
			if (session.Transaction.IsActive)
			{
				//Мне кажется этот код никогда не исполнится, todo подумать и удалить
				// 25.05.2015 Код все же выполняется. Но не могу точно объяснить когда он доходит до этого места,
				// а когда попадает в обработчик onException в BaseController
				if (exception != null)
					session.Transaction.Rollback();
				else {
					CommitAndAvoidDeadLocks(session);
				}
			}
		}

		private static void CommitAndAvoidDeadLocks(ISession session)
		{
			var iteration = 0;
			while (true)
			{
				iteration++;
				try
				{
					session.Transaction.Commit();
					return;
				}
				catch (Exception e)
				{
					var needToRepeat = iteration <= DeadlockWaitAttempts;
					if (!IsDeadLockOrSimilarExceptionInChain(e) || !needToRepeat)
						throw;
					var str = String.Format("Deadlock найден, пытаемся восстановить {0} из· {1} попыток. <br> {2}", iteration, DeadlockWaitAttempts,e.Message);
					EmailSender.SendError(str);
				}
				Thread.Sleep(DeadlockWaitTime);
			}
		}
		public static bool IsDeadLockOrSimilarExceptionInChain(Exception ex)
		{
			if (ex == null)
				return false;
			if (ex is MySqlException
				&& (((MySqlException)ex).Number == 1205
					|| ((MySqlException)ex).Number == 1213
					|| ((MySqlException)ex).Number == 1422))
				return true;
			return IsDeadLockOrSimilarExceptionInChain(ex.InnerException);
		}
	}
}