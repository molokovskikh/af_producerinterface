using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			// TODO список новостей
			return View("Index");
		}
	}
}