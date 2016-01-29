using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ReportModels;

namespace ProducerInterface.Controllers
{
    public class general_reportsController : Controller
    {
        private reportsEntities db = new reportsEntities();

        // GET: general_reports
        public ActionResult Index()
        {
            return View(db.general_reports.ToList());
        }

        // GET: general_reports/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            general_reports general_reports = db.general_reports.Find(id);
            if (general_reports == null)
            {
                return HttpNotFound();
            }
            return View(general_reports);
        }

		public ActionResult ReportDetails(decimal id)
		{
			var report = db.reports.Find(id);
			return View(report);

		}

        //// GET: general_reports/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: general_reports/Create
        //// Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        //// сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "GeneralReportCode,FirmCode,Allow,EMailSubject,ReportFileName,ReportArchName,ContactGroupId,Temporary,TemporaryCreationDate,Comment,PayerID,Format,NoArchive,OwnedByUser,SendDescriptionFile,Public,LastSuccess,PublicSubscriptionsId,MailPerFile")] general_reports general_reports)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.general_reports.Add(general_reports);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(general_reports);
        //}

        //// GET: general_reports/Edit/5
        //public ActionResult Edit(decimal id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    general_reports general_reports = db.general_reports.Find(id);
        //    if (general_reports == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(general_reports);
        //}

        //// POST: general_reports/Edit/5
        //// Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        //// сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "GeneralReportCode,FirmCode,Allow,EMailSubject,ReportFileName,ReportArchName,ContactGroupId,Temporary,TemporaryCreationDate,Comment,PayerID,Format,NoArchive,OwnedByUser,SendDescriptionFile,Public,LastSuccess,PublicSubscriptionsId,MailPerFile")] general_reports general_reports)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(general_reports).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(general_reports);
        //}

        //// GET: general_reports/Delete/5
        //public ActionResult Delete(decimal id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    general_reports general_reports = db.general_reports.Find(id);
        //    if (general_reports == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(general_reports);
        //}

        //// POST: general_reports/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(decimal id)
        //{
        //    general_reports general_reports = db.general_reports.Find(id);
        //    db.general_reports.Remove(general_reports);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
