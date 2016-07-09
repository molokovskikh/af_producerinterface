using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class HomeController : MasterBaseController
	{
		public ActionResult Index()
		{
			// TODO список новостей
			return View("Index");
		}
	}
}