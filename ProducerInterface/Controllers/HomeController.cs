using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
	public class HomeController : MasterBaseController
	{

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}

		public ActionResult Index()
		{
			// TODO список новостей
			return View("Index");
		}
	}
}