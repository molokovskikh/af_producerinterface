using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.Models;
using Test.Support.Selenium;

namespace test
{
	[TestFixture]
	public class MailFixture : SeleniumFixture
	{
		private readonly string username;

		public MailFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test(Description = "Добавление тэга в тело письма, сохранение письма")]
		public void MailBodyArgs()
		{
			Open();
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Шаблоны писем");
			Css("[name='id']").SelectByText(@MailType.EditPromotion.DisplayName());
			//Добавляем в шаблон редактирования тэг
			ClickButton("Редактировать шаблон");
			WaitForText("Изменения в промо-акции");
			var inputObj = browser.FindElementByCssSelector("textarea#Body");
			var textagain = inputObj.GetAttribute("value");
			inputObj.Clear();
			inputObj.SendKeys(textagain + "{Break}");
			ClickButton("Сохранить изменения");
			session = session.SessionFactory.OpenSession();
			session.Flush();
			var mailBody =
				session.Query<MailFormWithFooter>().First(s => s. Id == (int) @MailType.EditPromotion).Body;
			var dict = new Dictionary<string, object>();
			dict.Add("Break", "<br />");
			mailBody = EmailSender.Render(mailBody, dict);
			Assert.That(mailBody, Contains.Substring(dict["Break"].ToString()));
		}
	}
}