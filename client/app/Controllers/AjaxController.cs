using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.CatalogModels;
using assortment = ProducerInterfaceCommon.ContextModels.assortment;

namespace ProducerInterface.Controllers
{
	public class AjaxController : BaseController
	{
		public JsonResult GetMnn(string term)
		{
			var ccntx = new catalogsEntities();
			return
				Json(ccntx.mnn.Where(x => x.Mnn1.Contains(term)).Take(10).ToList().Select(x => new {value = x.Id, text = x.Mnn1}),
					JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetDisplayStatusJson(string jobName)
		{
			var displayStatus = DB.jobextend.Single(x => x.JobName == jobName).DisplayStatus;
			return Json(new {
				status = displayStatus,
				url = Url.Action("DisplayReport", "Report", new {jobName})
			});
		}

		private JsonResult ToJson(IQueryable<assortment> query)
		{
			var results = query
				.Take(20)
				.ToList()
				.Select(x => new {value = x.CatalogId, text = x.CatalogName})
				.Distinct()
				.Take(10)
				.ToList();
			return Json(results, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetProducts(string term)
		{
			var query = DB.assortment.Where(x => x.CatalogName.Contains(term));
			if (CurrentUser.IsProducer) {
				query = query.Where(x => x.ProducerId == CurrentUser.AccountCompany.ProducerId.Value);
			}
			return ToJson(query);
		}

		public JsonResult GetConcurrentProducts(string term)
		{
			var query = DB.assortment.Where(x => x.CatalogName.Contains(term))
				.Where(x => x.ProducerId != CurrentUser.AccountCompany.ProducerId.Value);
			return ToJson(query);
		}
	}
}