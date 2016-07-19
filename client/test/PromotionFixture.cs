using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;

namespace ProducerInterface.Test
{
	[TestFixture]
	public class PromotionFixture : BaseFixture
	{
		[Test]
		public void Register_promotion()
		{
			Open("/?debug-user=kvasovtest@analit.net");
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
	}
}