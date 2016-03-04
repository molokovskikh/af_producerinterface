using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class MediaFilesController : MasterBaseController
	{

		public ActionResult Index()
		{
			ViewBag.FullUrlStringFile = GetWebConfigParameters("ImageFullUrlString");

#if DEBUG
			{
				ViewBag.FullUrlStringFile = this.Request.Url.Segments[0] + @"mediafiles/";
			}
#endif

			var files = cntx_.MediaFiles.Where(x => x.EntityType == (int)EntityType.News).Select(x => x.Id).ToList();
			return View(files);
		}

		public ActionResult EmailFileList()
		{
			var files = cntx_.MediaFiles.Where(x => x.EntityType == (int)EntityType.Email).Select(x => x.Id).ToList();
			return View(files);
		}


		public FileResult GetFile(int Id)
		{
			var file = cntx_.MediaFiles.Find(Id);
			return File(file.ImageFile, file.ImageType);
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
			if (file != null && file.ContentLength > 0) {
				var fileId = SaveFile(file, EntityType.Email);
				SuccessMessage("Файл успешно загружен");
				return RedirectToAction("Index", "Mail");
      }
			else {
				ErrorMessage("Файл не найден");
				return RedirectToAction("Index", "Mail");
			}
		}

		[HttpPost]
		public void YourAction(IEnumerable<HttpPostedFileBase> files)
		{
			if (files != null)
			{
				foreach (var file in files)
				{
					// Verify that the user selected a file
					if (file != null && file.ContentLength > 0)
					{
						// extract only the fielname
						var fileName = Path.GetFileName(file.FileName);
						// TODO: need to define destination
						var path = Path.Combine(Server.MapPath("~/Upload"), fileName);
						file.SaveAs(path);
					}
				}
			}
		}


		private int SaveFile(HttpPostedFileBase file, EntityType type)
		{
			var dbFile = new MediaFiles();
			dbFile.ImageName = file.FileName.Split(new Char[] { '\\' }).Last();
			dbFile.ImageType = file.ContentType;
			dbFile.EntityType = (int)type;
			using (var ms = new MemoryStream())
			{
				file.InputStream.CopyTo(ms);
				dbFile.ImageFile = ms.ToArray();
				dbFile.ImageSize = ms.Length.ToString();
			}
			cntx_.MediaFiles.Add(dbFile);
			cntx_.SaveChanges();
			return dbFile.Id;
		}
	}
}