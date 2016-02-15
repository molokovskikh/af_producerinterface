using ProducerInterfaceCommon.CatalogModels;
using System.Linq;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class AjaxController : Controller
    {
			// GET: Ajax
			public JsonResult GetMnn(string term)
			{
				var ccntx = new catalogsEntities();
				var ret = Json(ccntx.mnn.Where(x => x.Mnn1.Contains(term)).Take(10).ToList().Select(x => new { value = x.Id, text = x.Mnn1 }), JsonRequestBehavior.AllowGet);
				return ret;
			}
	}
}