using System.Collections.Generic;
using System.Linq;
using NHibernate.Validator.Engine;

namespace AnalitFramefork.Components.Validation
{
	/// <summary>
	/// Перегрузка для списка ошибок, чтобы делать с ним дополнительные манипуляции
	/// </summary>
	public class ValidationErrors : List<InvalidValue>
	{
		public int Length
		{
			get { return this.Count; }
		}

		public ValidationErrors(IEnumerable<InvalidValue> ListOfErrors)
		{
			this.AddRange(ListOfErrors);
		}

		/// <summary>
		/// Удаление элемента из списка ошибок, появившихся в результате валидации 
		/// Пример использования RemoveErrors("Inforoom2.Models", "BirthDate")
		/// </summary>
		/// <param name="ClassName">Название класса</param>
		/// <param name="PropertyName">Название свойства</param>
		/// <returns>Список ошибок, появившихся в результате валидации</returns>
		public ValidationErrors RemoveErrors(string ClassName, string PropertyName)
		{
			this.RemoveAll(s => s.EntityType.Name.ToString() + "." + s.PropertyName == ClassName + "." + PropertyName);

			return this;
		}

		/// <summary>
		/// Удаление элементов из списка ошибок, появившихся в результате валидации 
		/// </summary>
		/// <param name="ErrorsToRemove">Строка в виде "RootEntity+"."+PropertyName"</param>
		/// <returns>Список ошибок, появившихся в результате валидации</returns>
		public ValidationErrors RemoveErrors(List<string> ErrorsToRemove)
		{
			foreach (var item in ErrorsToRemove)
			{
				var ElementToRemove = this.FirstOrDefault(s => s.RootEntity + "." + s.PropertyName == item);
				if (ElementToRemove != null)
				{
					this.Remove(ElementToRemove);
				}
			}
			return this;
		}
	}
}