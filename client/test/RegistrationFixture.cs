using System;
using System.Linq;
using NUnit.Framework;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class RegistrationFixture : BaseFixture
	{
		public RegistrationFixture()
		{
			defaultUrl = "/?debug-user=";
		}

		[Test]
		public void Register_not_producer()
		{
			Open();
			Click("Регистрация");
			Click("Компания в списке отсутствует");
			Css("#CompanyName").SendKeys("Тестовая организация");
			Css("#LastName").SendKeys("Тестовая организация");
			Css("#FirstName").SendKeys("Тестовая организация");
			Css("#register-form #login").SendKeys($"{Guid.NewGuid()}@analit.net");
			Css("#PhoneNumber").SendKeys("4732606000");
			Css("#Appointment").SendKeys("Тестовая должность");
			Click("Запрос на регистрацию");
			AssertText("Ваша заявка принята. Ожидайте, с вами свяжутся");
		}

		[Test]
		public void Recover_email()
		{
			Open();
			Click("Вход на сайт");
			Click("Забыли пароль?");
			AssertText("Восстановление пароля");
			Css("#password-recovery #login").SendKeys("r.kvasov@analit.net");
			Click("Восстановить");
			AssertNoText("Ошибка сервера в приложении");
			AssertText("Новый пароль отправлен на ваш email");
		}
	}
}