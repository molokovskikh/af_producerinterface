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
        public ActionResult Details(decimal? id)
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

    }
}
