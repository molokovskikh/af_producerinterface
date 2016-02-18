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

			var model = cntx_.mailform.Single(xxx => xxx.Id == id);
			return View(model);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Принимает шаблон письма для сохранения в БД</returns>
		[HttpPost]
		public ActionResult Edit(mailform mailForm)
		{
			cntx_.Entry(mailForm).State = System.Data.Entity.EntityState.Modified;
			cntx_.SaveChanges();
			SuccessMessage("Шаблон письма сохранен");
			return RedirectToAction("Index");
		}

	}
}