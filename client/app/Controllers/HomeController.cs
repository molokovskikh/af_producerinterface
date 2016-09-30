using System.Linq;
using System.Web.Mvc;
using NHibernate.Linq;
using ProducerInterfaceCommon.Models;

namespace ProducerInterface.Controllers
{
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			if (CurrentUser != null)
				return RedirectToAction("Index", "Profile");
			var listResult = DbSession.Query<Slide>().Where(s => s.Enabled).OrderByDescending(s => s.PositionIndex).ToList();
			return View(listResult);
		}
	}
}