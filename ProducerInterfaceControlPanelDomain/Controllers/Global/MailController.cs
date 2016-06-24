using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class MailController : MasterBaseController
	{
		/// <summary>
		/// Возвращает список шаблонов писем GET: Mail
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			var model = cntx_.MediaFiles.Where(x => x.EntityType == (int)EntityType.Email)
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

			var files = cntx_.MediaFiles.Where(x => ids.Contains(x.Id));
			foreach (var file in files)
				cntx_.MediaFiles.Remove(file);
			cntx_.SaveChanges();
			return RedirectToAction("Index");
		}

		public ActionResult DeleteLinksToFile(int Id, List<int> fileId)
		{
			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == Id);
			var mediaFiles = cntx_.MediaFiles.Where(x => fileId.Contains(x.Id)).ToList();
			foreach (var f in mediaFiles)
				mailForm.MediaFiles.Remove(f);
			cntx_.SaveChanges();
			return RedirectToAction("Edit", new { id = Id });
		}

		/// <summary>
		/// Возврашает Шаблон выбранного письма для правки
		/// </summary>
		/// <param name="id">идентификатор формы</param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Edit(int? id)
		{
			if (!id.HasValue)
				return RedirectToAction("Index");

			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == id);
			if (mailForm == null)
				return RedirectToAction("Index");

			// файлы, присоединенные к форме
			var mediaFiles = mailForm.MediaFiles
				.Select(x => new { x.Id, x.ImageName })
				.ToList()
				.Select(x => Tuple.Create(x.Id, x.ImageName))
				.ToList();

			var mediaFilesIds = mediaFiles.Select(y => y.Item1).ToList();

			// все доступные файлы, кроме уже присоединенных
			var allMediaFiles = cntx_.MediaFiles
				.Where(x => x.EntityType == (int)EntityType.Email && !mediaFilesIds.Contains(x.Id))
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
		public ActionResult AttachFile(int Id, List<int> fileId)
		{
			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == Id);
			if (mailForm == null)
				return RedirectToAction("Index");

			var mediaFiles = cntx_.MediaFiles.Where(x => fileId.Contains(x.Id)).ToList();
			foreach (var f in mediaFiles)
				mailForm.MediaFiles.Add(f);
			cntx_.SaveChanges();

			return RedirectToAction("Edit", new { id = Id });
		}

		/// <summary>
		/// Принимает шаблон письма для сохранения в БД
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Edit(MailFormUi model)
		{
			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == model.Id);
			if (mailForm == null)
				return RedirectToAction("Index");

			mailForm.Body = model.Body;
			mailForm.Subject = model.Subject;
			cntx_.SaveChanges();

			return RedirectToAction("Edit", new { id = model.Id });
		}

	}
}