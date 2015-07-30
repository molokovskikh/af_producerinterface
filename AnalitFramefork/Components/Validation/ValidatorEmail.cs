using System.Text.RegularExpressions;

namespace AnalitFramefork.Components.Validation
{
	//Валидация контактов
	public class ValidatorEmail : CustomValidator
	{
		// перечень проверок
		private static readonly Regex CheckMailFormat = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#" +
						@"$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
						RegexOptions.IgnoreCase);  

		protected override void Run(object value)
		{  

			if (value is string) {

				// проверка NotEmpty
				if (value == string.Empty)
				{ 
					return;
				}
				if (!CheckMailFormat.Match(value as string).Success)
				{
					AddError("<strong class='msg'>Адрес email указан неверно</strong>");
				}
				
			}

		}
	}
}