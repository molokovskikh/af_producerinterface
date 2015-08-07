using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalitFramefork.Components;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ReportsControlPanel.Models;

namespace ReportsControlPanel.Controllers
{
    public class GeneralReportsController : BaseController
    {
        //
        // GET: /Home/

		public ActionResult GeneralReportList()
		{
			var filter = new ModelFilter<GeneralReport>(this);
			ViewBag.Filter = filter;
			return View();
		}

    }
}
