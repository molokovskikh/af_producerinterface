using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			if (CurrentUser != null)
				return RedirectToAction("Index", "Profile");
			return View("Index");
		}
	}
}