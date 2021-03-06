﻿using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class PromotionFixture : BaseFixture
	{
		private string username;

		public PromotionFixture()
		{
			username = "r.kvasov@analit.net";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void Register_promotion()
		{
			Open();
			Click("Акции");
			var navbar = browser.FindElementsByCssSelector("div.navbar-collapse.collapse").First();
			int navbarHeight = navbar.Size.Height;
			var elAction = (IWebElement)Css("#AddItem");
			ScrollTo(elAction, yAdditive: -navbarHeight);
			Click("Добавить акцию",false);
			WaitAjax();
			AssertText("Новая промоакция");
			Css("#Name").SendKeys("test");
			Css("#Annotation").SendKeys("test");
			//Воронеж
			ChoseRegion("#RegionList");
			//АРИПРИЗОЛ табл. 10 мг N30
			Eval("$('#DrugList').val('208437').trigger('chosen:updated').change();");
			var el = (IWebElement)Css("#all-suppliers");
			ScrollTo(el, yAdditive: -navbarHeight);
			el.Click();
			while (true) {
				try {
					Click("Добавить и отправить запрос на подтверждение");
					break;
				} catch (StaleElementReferenceException) {
				}
			}
			AssertText("Промо акция добавлена и отправлен запрос на её подтверждение");
		}

		[Test]
		public void Delete()
		{
			var promotion = new Promotion(db.Account.First(x => x.Login == username));
			promotion.Name = Guid.NewGuid().ToString();
			promotion.Annotation = promotion.Name;
			promotion.Author = db2.Users.First(x => x.Login == username);
			promotion.MediaFile = new MediaFile("test.png") {
				ImageFile = new byte[100],
				ImageSize = 10,
				ImageType = "image/png",
				EntityType = EntityType.Promotion,
			};
			db2.Promotions.Add(promotion);
			db2.SaveChanges();

			Open();
			Click("Акции");
			WaitForText(promotion.Name, 20);
			var navbar = browser.FindElementsByCssSelector("div.navbar-collapse.collapse").First();
			int navbarHeight = navbar.Size.Height;
			var el = browser.FindElementsByCssSelector("a").First(x => x.Text?.Trim().StartsWith(promotion.Name) == true);
			ScrollTo(el, yAdditive: -navbarHeight);
			el.Click();
			Thread.Sleep(100);
			WaitAnimation();
			WaitAjax();
			var elAction = (IWebElement)Css("#ButtonDelete");
			ScrollTo(elAction, yAdditive: -navbarHeight);
			elAction.Click();
			//подтверждение удаления
			ConfirmDialog("Вы уверены, что хотите удалить акцию");
			AssertText("Акция удалена");
		}
	}
}