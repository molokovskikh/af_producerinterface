using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class MailController : MasterBaseController
	{
		// GET: Mail
		/// <summary>
		/// 
		/// </summary>
		/// <returns>Возвращает список шаблонов писем</returns>
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Возврашает Шаблон выбранного письма для правки</returns>
		[HttpGet]
		public ActionResult Edit(int? id)
		{
			if (!id.HasValue)
				return RedirectToAction("Index");

			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == id);
			if (mailForm == null)
				return RedirectToAction("Index");

			var mediaFiles = mailForm.MediaFiles.Select(x => x.Id).ToList();
			var model = new MailFormUi() { Body = mailForm.Body, Description = mailForm.Description, Id = mailForm.Id, Subject = mailForm.Subject, MediaFiles = mediaFiles };
      return View(model);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Принимает шаблон письма для сохранения в БД</returns>
		[HttpPost]
		public ActionResult Edit(MailFormUi model)
		{
			var mailForm = cntx_.mailform.SingleOrDefault(x => x.Id == model.Id);
			if (mailForm == null)
				return RedirectToAction("Index");

			mailForm.Body = model.Body;
			mailForm.Subject = model.Subject;

			if (model.AddMediaFiles != null && model.AddMediaFiles.Count > 0) {
				var addMediaFiles = cntx_.MediaFiles.Where(x => model.AddMediaFiles.Contains(x.Id)).ToList();
				foreach (var addMediaFile in addMediaFiles)
					mailForm.MediaFiles.Add(addMediaFile);
			}

			if (model.RemoveMediaFiles != null && model.RemoveMediaFiles.Count > 0) {
				var removeMediaFiles = cntx_.MediaFiles.Where(x => model.RemoveMediaFiles.Contains(x.Id)).ToList();
				foreach (var removeMediaFile in removeMediaFiles)
					mailForm.MediaFiles.Remove(removeMediaFile);
			}

			cntx_.SaveChanges();
			SuccessMessage("Шаблон письма сохранен");
			return RedirectToAction("Index");
		}

	}
}