using System.Web.Mvc;
using System.Linq;
using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Controllers;

namespace ProducerInterface.Controllers
{
	public class MasterBaseController : BaseController
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			TypeLoginUser = TypeUsers.ProducerUser;
			base.OnActionExecuting(filterContext);

			if (CurrentUser != null)
			{
				if (CurrentUser.AccountCompany.ProducerId != null)
					ViewBag.Producernames = cntx_.producernames.Single(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId).ProducerName;
				else
					ViewBag.Producernames = "Физическое лицо";
			}
		}
	}
}
