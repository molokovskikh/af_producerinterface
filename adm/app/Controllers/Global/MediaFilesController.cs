using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class MediaFilesController : BaseController
	{

		public ActionResult Index()
		{
			ViewBag.FullUrlStringFile = GetWebConfigParameters("ImageFullUrlString");

#if DEBUG
			{
				ViewBag.FullUrlStringFile = this.Request.Url.Segments[0] + @"mediafiles/";
			}
#endif

			var files = DB2.MediaFiles.Where(x => x.EntityType == EntityType.News).Select(x => x.Id).ToList();
			return View(files);
		}

		public FileResult GetFile(int id)
		{
			var file = DB2.MediaFiles.Find(id);
			return File(file.ImageFile, file.ImageType);
		}

		public FileResult GetEmailFile(int id)
		{
			var imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content\\Images");
			var file = DB2.MediaFiles.Find(id);
			var ext = Path.GetExtension(file.ImageName)?.ToLower();

			switch (ext) {
				case ".jpg":
				case ".png":
				case ".gif":
				case ".bmp":
					return File(file.ImageFile, file.ImageType);
				case ".txt":
					return File(Path.Combine(imageDir, "txt.png"), "image/png");
				case ".xls":
				case ".xlsx":
					return File(Path.Combine(imageDir, "xls.png"), "image/png");
				case ".zip":
					return File(Path.Combine(imageDir, "zip.png"), "image/png");
				default:
					return File(Path.Combine(imageDir, "other.png"), "image/png");
			}
		}

		public ActionResult SaveNewsFile()
		{
			var file = Request.Files[0];
			var fileId = SaveFile(file, EntityType.News);
			string ckEditorFuncNum = HttpContext.Request["CKEditorFuncNum"];
			string url = $"{GetWebConfigParameters("ImageFullUrlString")}/GetFile/{fileId}";
			return Content($"<script>window.parent.CKEDITOR.tools.callFunction({ckEditorFuncNum}, \"{url}\");</script>");
		}

		[HttpPost]
		public ActionResult SaveEmailFile(HttpPostedFileBase file)
		{
			if (file == null || file.ContentLength == 0) {
				ErrorMessage("Файл не найден");
				return RedirectToAction("Index", "Mail");
			}

			var fileName = Path.GetFileName(file.FileName);
			if (DB2.MediaFiles.Any(x => x.ImageName == fileName && x.EntityType == EntityType.Email)) {
				ErrorMessage("В системе уже есть файл с таким именем. Переименуйте этот или удалите существующий");
				return RedirectToAction("Index", "Mail"); ;
			}

			SaveFile(file, EntityType.Email);
			return RedirectToAction("Index", "Mail");
		}

		private int SaveFile(HttpPostedFileBase file, EntityType type)
		{
			var model = new MediaFile(file.FileName) {
				ImageType = file.ContentType,
				EntityType = type
			};
			using (var ms = new MemoryStream())
			{
				file.InputStream.CopyTo(ms);
				model.ImageFile = ms.ToArray();
				model.ImageSize = ms.Length.ToString();
			}
			DB2.MediaFiles.Add(model);
			DB2.SaveChanges();
			return model.Id;
		}
	}
}