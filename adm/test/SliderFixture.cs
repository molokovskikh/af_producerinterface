using System;
using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using ProducerInterfaceCommon.Models;
using Test.Support.Selenium;

namespace test
{
	public class SliderFixture : BaseFixture
	{
		private readonly string username;

		public SliderFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void SlideListNavigationCheck()
		{
			var list = session.Query<Slide>().ToList();
			list.ForEach(s => { session.Delete(s); });
			for (var i = 0; i < 3; i++) {
				session.Save(new Slide {ImagePath = i, Enabled = true, LastEdit = SystemTime.Now()});
			}
			session.Flush();
			list = session.Query<Slide>().ToList();
			Assert.That(list.Count, Is.EqualTo(3));
			Open();
			AssertText("Статистика");
			Click("Разделы сайта");
			Click("Слайдер");
			WaitForText("Список слайдов");
			foreach (var item in list) {
				AssertText(item.Id.ToString());
			}
			ClickLink("Удалить");
			WaitForText("Список слайдов");
			session.Flush();
			list = session.Query<Slide>().ToList();
			Assert.That(list.Count, Is.EqualTo(2));
			foreach (var item in list) {
				AssertText(item.Id.ToString());
			}
			ClickLink("Изменить");
			WaitForVisibleCss(".btn-success");
			ClickButton("Сохранить");
			WaitForText("Список слайдов");
			WaitForText("Баннер успешно сохранен");
		}

		[Test]
		public void SlideWithoutImageError()
		{
			var list = session.Query<Slide>().ToList();
			list.ForEach(s => { session.Delete(s); });
			Open();
			Open("Slide");
			WaitForText("Список слайдов");
			WaitForVisibleCss(".btn-success");
			ClickLink("Добавить");
			WaitForVisibleCss("[name='uploadedFile']");
			ClickButton("Добавить");
			WaitForText("не задано изображение");
		}
	}
}