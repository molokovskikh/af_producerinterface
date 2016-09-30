using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Helpers;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class SlideController : BaseController
	{
		/// <summary>
		///   Список слайдов
		/// </summary>
		public ActionResult Index()
		{
			var listResult = DbSession.Query<Slide>().OrderByDescending(s => s.PositionIndex).ToList();
			return View(listResult);
		}


		/// <summary>
		///   Форма добавление слайда
		/// </summary>
		[HttpGet]
		public ActionResult CreateSlide()
		{
			//Создаем слайд
			var Slide = new Slide {Enabled = false};
			return View(Slide);
		}

		/// <summary>
		///   Добавление слайда в базу
		/// </summary>
		[HttpPost]
		public ActionResult CreateSlide(Slide slide, HttpPostedFileBase uploadedFile)
		{
			var ext = uploadedFile == null ? "" : new FileInfo(uploadedFile.FileName).Extension;
			if (ext == ".png" || ext == ".jpg" || ext == ".jpeg") {
				slide.ImagePath = FileManager.SaveFile(DB2, uploadedFile, EntityType.Slide);
			}
			slide.LastEdit = DateTime.Now;
			if (slide.ImagePath.HasValue) {
				DbSession.Save(slide);
				SuccessMessage("Баннер успешно добавлен");
				return RedirectToAction("Index");
			}
			ErrorMessage("Баннер не может быть добавлен: не задано изображение.");
			return View(slide);
		}


		/// <summary>
		///   Просмотр слайда
		/// </summary>
		[HttpGet]
		public ActionResult EditSlide(int id)
		{
			//Создаем слайд
			var slide = DbSession.Query<Slide>().FirstOrDefault(s => s.Id == id);
			return View(slide);
		}

		/// <summary>
		///   Изменение слайда
		/// </summary>
		[HttpPost]
		public ActionResult EditSlide(Slide slide, HttpPostedFileBase uploadedFile)
		{
			slide = slide.UpdateAndGetIfExists(DbSession);
			var ext = uploadedFile == null ? "" : new FileInfo(uploadedFile.FileName).Extension;
			if (ext == ".png" || ext == ".jpg" || ext == ".jpeg") {
				if (slide.ImagePath.HasValue) {
					FileManager.DeleteFile(DB2, slide.ImagePath.Value);
				}
				slide.ImagePath = FileManager.SaveFile(DB2, uploadedFile, EntityType.Slide);
			}
			if (slide.ImagePath.HasValue) {
				DbSession.Save(slide);
				SuccessMessage("Баннер успешно сохранен");
				return RedirectToAction("Index");
			}
			return View(slide);
		}


		/// <summary>
		///   Удаление баннера
		/// </summary>
		public ActionResult DeleteSlide(int id)
		{
			var slide = DbSession.Query<Slide>().FirstOrDefault(s => s.Id == id);
			if (slide.ImagePath.HasValue) {
				FileManager.DeleteFile(DB2, slide.ImagePath.Value);
			}
			DbSession.Delete(slide);
			DbSession.Flush();
			SuccessMessage("Баннер успешно удален");
			return RedirectToAction("Index");
		}

		private bool updatePositionIndex(uint[] idList)
		{
			var listResult = DbSession.Query<Slide>().ToList();
			if (idList == null) {
				for (var i = 0; i < listResult.Count; i++) {
					listResult[i].PositionIndex = null;
					DbSession.Save(listResult[i]);
				}
			} else {
				if (idList.Length != listResult.Count) {
					return false;
				}
				for (uint i = 0; i < idList.Length; i++) {
					var currentElement = listResult.FirstOrDefault(s => s.Id == idList[i]);
					if (currentElement != null) {
						currentElement.PositionIndex = i;
					} else {
						DbSession.Transaction.Rollback();
						return false;
					}
					DbSession.Save(currentElement);
				}
			}
			return true;
		}

		public JsonResult UpdatePositionIndex(uint[] idList)
		{
			const string error = "Изменяемый список был обновлен и не соответствует текущему.";
			if (idList == null) {
				return Json(error, JsonRequestBehavior.AllowGet);
			}
			var result = updatePositionIndex(idList);
			return Json(result ? string.Empty : error, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UpdatePositionIndexReset()
		{
			updatePositionIndex(null);
			return RedirectToAction("Index");
		}

		public FileResult GetFile(int id)
		{
			var file = DB2.MediaFiles.Find(id);
			return File(file.ImageFile, file.ImageType);
		}
	}
}