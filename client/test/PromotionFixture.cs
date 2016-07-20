using System;
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
			username = "kvasovtest@analit.net";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void Register_promotion()
		{
			Open();
			Click("Акции");
			Click("Добавить акцию");
			WaitAjax();
			AssertText("Новая промоакция");
			Css("#Name").SendKeys("test");
			Css("#Annotation").SendKeys("test");
			//Воронеж
			Eval("$('#RegionList').val('1').trigger('chosen:updated').change();");
			//БЕТАСЕРК табл. 16 мг N30
			Eval("$('#DrugList').val('3677').trigger('chosen:updated').change();");
			Css("#all-suppliers").Click();
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
			var el = browser.FindElementsByCssSelector("a").First(x => x.Text?.Trim().StartsWith(promotion.Name) == true);
			ScrollTo(el);
			el.Click();
			Thread.Sleep(100);
			WaitAnimation();
			var delete = browser.FindElementsByCssSelector("a.delete")
				.First(x => x.GetAttribute("href").EndsWith($"/{promotion.Id}"));
			ScrollTo(delete);
			delete.Click();
			var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(3));
			wait.Until(ExpectedConditions.AlertIsPresent());
			browser.SwitchTo().Alert().Accept();
			AssertText("Акция удалена");
		}
	}
}