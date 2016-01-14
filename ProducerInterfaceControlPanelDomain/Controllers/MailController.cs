using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceControlPanelDomain.Models;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class MailController : BaseController
    {
        // GET: Mail
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Возвращает список шаблонов писем</returns>
        public ActionResult Index()
        {
            var MailFormModel = cntx_.mailform.ToList();                  
            return View(MailFormModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Возврашает Шаблон выбранного письма для правки</returns>
        [HttpGet]
        public ActionResult Edit(int Id)
        {
            var MailFormModel = cntx_.mailform.Where(xxx => xxx.Id == Id).First();
            return View(MailFormModel);
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
            return RedirectToAction("Index");
        }

    }
}