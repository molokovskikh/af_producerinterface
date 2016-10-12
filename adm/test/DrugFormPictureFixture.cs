using System;
using System.Linq;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using Test.Support.Selenium;
using CatalogLog = ProducerInterfaceCommon.Models.CatalogLog;
using MediaFiles = ProducerInterfaceCommon.Models.MediaFiles;

namespace test
{
	public class DrugFormPictureFixture : BaseFixture
	{
		private readonly string username;

		public DrugFormPictureFixture()
		{
			username = "kvasov";
			defaultUrl = $"/?debug-user={username}";
		}

		[Test]
		public void DrugFormPictureApply()
		{
			//значения по умолчанию
			var catalogId = session.Query<Catalog>().First().Id;
			var producerId = session.Query<Producer>().First().Id;
			var drugFamilyId = session.Query<CatalogNames>().First().Id;
			//очистка списка изображений для спецификаций
			var list =
				session.Query<DrugFormPicture>().Where(x => x.CatalogId == catalogId && x.ProducerId == producerId).ToList();
			list.ForEach(s => { session.Delete(s); });
			//Добавление предложения смены изображения, получение Id отображаемого изображения
			var pictureId = ProposalOfDrugFormPictureAdd(catalogId, Convert.ToUInt32(producerId), drugFamilyId);

			DrugFormPictureListPrepare(catalogId, producerId, pictureId);

			//принятие правки
			ClickLink("Принять");
			AssertText("Правки успешно внесены в каталог");
			//проверка результата принятия правки
			var drugFormPicture =
				session.Query<DrugFormPicture>().First(x => x.CatalogId == catalogId && x.ProducerId == producerId);
			session.Refresh(drugFormPicture);
			Assert.IsTrue(drugFormPicture.PictureKey == pictureId);
			var LastCatalogLog = session.Query<CatalogLog>().OrderByDescending(s => s.Id).First();
			//статус предложения правки - принято
			session.Refresh(LastCatalogLog);
			Assert.IsTrue(LastCatalogLog.Apply == 1);

			//проверка фильтра
			WaitForText("Запросы правок каталога");
			Click(By.CssSelector("input[data-target='#filterModal']"));
			Css("#filterModal #Apply").SelectByText("Принятые");
			Click(By.CssSelector("input[onclick='getSearch()']"));

			//проверка перехода на страницу истории правок
			WaitForText("Запросы правок каталога");
			Click(By.CssSelector("a[onclick*='bindDetailsItem']"));
			WaitForText("Правка №");
			Click(By.CssSelector("div[data-bind='html:DateEditUi'] a"));
			WaitForText("История правок");
			//проверка наличия последней принятой правки в списке  истории
			var imgList = browser.FindElementsByCssSelector("#Table-LogUserChange img.max-width-400");
			//до - по умолчанию
			var item = imgList[0].GetAttribute("src");
			Assert.IsTrue(item.Contains("/Content/Images/no_image.png"));
			//после - изображение
			item = imgList[1].GetAttribute("src");
			Assert.IsTrue(item.Split('/').Any(s => s == pictureId.ToString()));
		}

		[Test]
		public void DrugFormPictureReject()
		{
			//значения по умолчанию
			var catalogId = session.Query<Catalog>().First().Id;
			var producerId = session.Query<Producer>().First().Id;
			var drugFamilyId = session.Query<CatalogNames>().First().Id;
			//очистка списка изображений для спецификаций
			var list =
				session.Query<DrugFormPicture>().Where(x => x.CatalogId == catalogId && x.ProducerId == producerId).ToList();
			list.ForEach(s => { session.Delete(s); });
			//Добавление предложения смены изображения, получение Id отображаемого изображения
			var pictureId = ProposalOfDrugFormPictureAdd(catalogId, Convert.ToUInt32(producerId), drugFamilyId);

			DrugFormPictureListPrepare(catalogId, producerId, pictureId);

			//принятие правки
			Click(By.CssSelector(".btn.btn-info.btn-sm[value='Отклонить']"));
			WaitForVisibleCss("#commentModal textarea");
			var inputObj = browser.FindElementByCssSelector("#commentModal textarea");
			inputObj.Clear();
			inputObj.SendKeys("Так надо");

			Click(By.CssSelector("#commentModal .btn.btn-primary[value='Отклонить']"));

			WaitForText("Комментарий отправлен пользователю");
			//проверка результата принятия правки
			var drugFormPicture =
				session.Query<DrugFormPicture>().First(x => x.CatalogId == catalogId && x.ProducerId == producerId);
			session.Refresh(drugFormPicture);
			Assert.IsTrue(drugFormPicture.PictureKey == null);
			var LastCatalogLog = session.Query<CatalogLog>().OrderByDescending(s => s.Id).First();
			//статус предложения правки - отклонено
			session.Refresh(LastCatalogLog);
			Assert.IsTrue(LastCatalogLog.Apply == 2);
		}


		private int ProposalOfDrugFormPictureAdd(uint catalogId, uint producerId, uint drugFamilyId)
		{
			var fileInDb = session.Query<MediaFiles>().FirstOrDefault() ??
				new ProducerInterfaceCommon.Models.MediaFiles() {
					EntityType = EntityType.DrugFormPicture,
					ImageName = "newFile",
					ImageSize = 101613,
					ImageType = "image/jpeg"
				};
			session.Save(fileInDb);
			var photo = new DrugFormPicture();
			photo.CatalogId = catalogId;
			photo.ProducerId = producerId;
			session.Save(photo);

			var model = session.Query<Catalog>().First(x => x.Id == catalogId);

			var dl = new CatalogLog() {
				After = fileInDb.Id.ToString(),
				Before = photo?.PictureKey?.ToString(),
				ObjectReference = catalogId,
				ObjectReferenceNameUi = model.Name,
				Type = (int) CatalogLogType.Photo,
				LogTime = DateTime.Now,
				OperatorHost = "0.0.0.1",
				UserId = session.Query<ProducerInterfaceCommon.Models.Account>().First(s => s.Login.IndexOf("@") != -1).Id,
				PropertyName = "PictureKey",
				PropertyNameUi = "Изображение",
				NameId = drugFamilyId,
				ProducerId = producerId
			};
			session.Save(dl);
			return fileInDb.Id;
		}

		private void DrugFormPictureListPrepare(uint catalogId, int producerId, int pictureId)
		{
			//проверяем отсутствие в нем изображения
			var drugFormPicture =
				session.Query<DrugFormPicture>().First(x => x.CatalogId == catalogId && x.ProducerId == producerId);
			Assert.IsTrue(drugFormPicture.PictureKey == null);
			//переходим на страницу запросов правок каталога
			Open();
			AssertText("Статистика");
			Click("Администрирование");
			Click("Запросы правок каталога");
			WaitForText("Запросы правок каталога");
			//получение всех изображений, проверка изображений
			var imgList = browser.FindElementsByCssSelector("#Table-LogUserChange img.max-width-400");
			//до - по умолчанию
			var item = imgList[0].GetAttribute("src");
			Assert.IsTrue(item.Contains("/Content/Images/no_image.png"));
			//после - изображение
			item = imgList[1].GetAttribute("src");
			Assert.IsTrue(item.Split('/').Any(s => s == pictureId.ToString()));
		}
	}
}