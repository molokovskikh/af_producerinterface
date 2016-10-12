using System;
using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Test
{
	public class DrugFormPictureFixture : BaseFixture
	{
		public DrugFormPictureFixture()
		{
			var username = "r.kvasov@analit.net";
			defaultUrl = $"/?debug-user={username}";
		}

		private int DrugFormPicture(uint catalogId, uint producerId, uint drugFamilyId)
		{
			var fileInDb = session.Query<ProducerInterfaceCommon.Models.MediaFiles>().FirstOrDefault() ??
			new ProducerInterfaceCommon.Models.MediaFiles() { EntityType = EntityType.DrugFormPicture, ImageName = "newFile", ImageSize = 101613, ImageType = "image/jpeg" };
			session.Save(fileInDb);
			var photo = new DrugFormPicture();
			photo.CatalogId = catalogId;
			photo.ProducerId = producerId;
			photo.PictureKey = fileInDb.Id;
			session.Save(photo);
			return fileInDb.Id;
		}

		[Test]
		public void DrugFormPictureAddingError()
		{
			//очистка списка изображений для спецификаций
			var list = session.Query<DrugFormPicture>().ToList();
			list.ForEach(s => { session.Delete(s); });
			//переход на страницу с выбором изображений для спецификаций
			Open();
			AssertText("Здраствуйте");
			Open("/Drug/Index/");
			WaitForText("Формы выпуска и дозировка");
			ClickLink("Формы выпуска и дозировка");
			var element = (IWebElement)Css(".listItem a img");
			//проверка отсутствия изображения
			var item = element.GetAttribute("src");
			Assert.IsTrue(item.Contains("/Content/Image/no_image.png"));
			//получение свойств для добавления "предлажения" нового изображения
			element = (IWebElement)Css(".listItem a");
			var catalogId = uint.Parse(element.GetAttribute("cId"));
			var producerId = uint.Parse(element.GetAttribute("pId"));
			var drugFamilyId = uint.Parse(element.GetAttribute("dId"));
			//добавление нового изображения (уже подтвержденного)
			int fileInDb = DrugFormPicture(catalogId, producerId, drugFamilyId);
			//провера заполнения наличия добавленного изображения на форме
			Open("/Drug/Index/");
			WaitForText("Формы выпуска и дозировка");
			ClickLink("Формы выпуска и дозировка");
			element = (IWebElement)Css(".listItem a");
			var imgUrl = element.GetAttribute("iUrl");
			Assert.IsTrue(imgUrl.IndexOf(fileInDb.ToString(), StringComparison.Ordinal) != -1);
			//неудачная попытка добавить предложение нового изображения без изображения
			element.Click();
			WaitForVisibleCss("#PictureUploadModalDialog .btn.btn-primary");
			Click(By.CssSelector("#PictureUploadModalDialog .btn.btn-primary"));
			WaitForText("Данный файл не может быть использован: допустимые форматы", 20);
		}
	}
}