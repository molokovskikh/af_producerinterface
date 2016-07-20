using System;
using System.Linq;
using NUnit.Framework;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class RegistrationFixture : BaseFixture
	{
		[Test]
		public void Register_not_producer()
		{
			Open("/?debug-user=");
			Click("Регистрация");
			Click("Компания в списке отсутствует");
			Css("#CompanyName").SendKeys("Тестовая организация");
			Css("#LastName").SendKeys("Тестовая организация");
			Css("#FirstName").SendKeys("Тестовая организация");
			Css("#register-form #login").SendKeys($"{Guid.NewGuid()}@analit.net");
			Css("#PhoneNumber").SendKeys("4732606000");
			Css("#Appointment").SendKeys("Тестовая должность");
			SafeClick("Запрос на регистрацию");
			AssertText("Ваша заявка принята. Ожидайте, с вами свяжутся");
		}

		[Test]
		public void Recover_email()
		{
			Open("/?debug-user=");
			Click("Вход на сайт");
			Click("Забыли пароль?");
			AssertText("Восстановление пароля");
			Css("#password-recovery #login").SendKeys("kvasovtest@analit.net");
			SafeClick("Восстановить");
			AssertNoText("Ошибка сервера в приложении");
			AssertText("Новый пароль отправлен на ваш email");
		}

		protected void SafeClick(string text)
		{
			var buttons = browser.FindElementsByCssSelector("a, input[type=button], input[type=submit], button");

			var button = buttons.FirstOrDefault(b => String.Equals(b.GetAttribute("value"), text, StringComparison.CurrentCultureIgnoreCase)) ??
				buttons.FirstOrDefault(b => String.Equals(b.Text?.Trim(), text, StringComparison.CurrentCultureIgnoreCase));

			if(button == null)
				throw new Exception($"Элемент с текстом '{text}' не найден!");

			if (button.Displayed)
				ScrollTo(button);
			button.Click();
		}
	}
}