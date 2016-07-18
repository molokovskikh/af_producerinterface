using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class MailController : BaseController
	{
		/// <summary>
		/// Возвращает список шаблонов писем GET: Mail
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = DB2.MediaFiles.Where(x => x.EntityType == EntityType.Email)
				.Select(x => new { x.Id, x.ImageName })
				.ToList()
				.Select(x => Tuple.Create(x.Id, x.ImageName))
				.ToList();
			return View(model);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public ActionResult DeleteFile(List<int> ids)
		{
			if (ids == null || !ids.Any()) {
				return RedirectToAction("Index");
			}

			var files = DB2.MediaFiles.Where(x => ids.Contains(x.Id));
			foreach (var file in files)
				DB2.MediaFiles.Remove(file);
			DB2.SaveChanges();
			return RedirectToAction("Index");
		}

		public ActionResult DeleteLinksToFile(int id, List<int> fileId)
		{
			var mailForm = DB2.Emails.Find(id);
			var mediaFiles = DB2.MediaFiles.Where(x => fileId.Contains(x.Id)).ToList();
			foreach (var f in mediaFiles)
				mailForm.MediaFiles.Remove(f);
			DB2.SaveChanges();
			return RedirectToAction("Edit", new { id });
		}

		/// <summary>
		/// Возврашает Шаблон выбранного письма для правки
		/// </summary>
		/// <param name="id">идентификатор формы</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Edit(int id)
		{
			var mailForm = DB2.Emails.Find(id);
			// файлы, присоединенные к форме
			var mediaFiles = mailForm.MediaFiles
				.Select(x => new { x.Id, x.ImageName })
				.ToList()
				.Select(x => Tuple.Create(x.Id, x.ImageName))
				.ToList();

			var mediaFilesIds = mediaFiles.Select(y => y.Item1).ToList();

			// все доступные файлы, кроме уже присоединенных
			var allMediaFiles = DB2.MediaFiles
				.Where(x => x.EntityType == EntityType.Email && !mediaFilesIds.Contains(x.Id))
				.Select(x => new {x.Id, x.ImageName})
				.ToList()
				.Select(x => Tuple.Create(x.Id, x.ImageName))
				.ToList();

			var model = new MailFormUi() {
				Body = mailForm.Body,
				Description = mailForm.Description,
				Id = mailForm.Id,
				Subject = mailForm.Subject,
				MediaFiles = mediaFiles,
				AllMediaFiles = allMediaFiles
			};
			return View(model);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public ActionResult AttachFile(int id, List<int> fileId)
		{
			var mailForm = DB2.Emails.Find(id);

			var mediaFiles = DB2.MediaFiles.Where(x => fileId.Contains(x.Id)).ToList();
			foreach (var f in mediaFiles)
				mailForm.MediaFiles.Add(f);
			DB2.SaveChanges();

			return RedirectToAction("Edit", new { id });
		}

		/// <summary>
		/// Принимает шаблон письма для сохранения в БД
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Edit(MailFormUi model)
		{
			var mailForm = DB2.Emails.SingleOrDefault(x => x.Id == model.Id);
			if (mailForm == null)
				return RedirectToAction("Index");

			mailForm.Body = model.Body;
			mailForm.Subject = model.Subject;
			DB2.SaveChanges();

			return RedirectToAction("Edit", new { id = model.Id });
		}

	}
}