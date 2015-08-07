using System.Web.Mvc;
using AnalitFramefork.Mvc;

namespace ReportsControlPanel.Controllers
{
    public class HomeController : BaseController
    {

		public ActionResult Index()
	{
		return RedirectToAction("GeneralReportList", "GeneralReports");
	}
}
}
