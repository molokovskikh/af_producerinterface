using ProducerInterfaceCommon.CatalogModels;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

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

		/// <summary>
		/// Возвращает статус указанного задания и ссылку на последний отчет
		/// </summary>
		/// <param name="jobName">Имя задания в Quartz</param>
		/// <param name="jobGroup">Группа задания в Quartz</param>
		/// <returns></returns>
		public JsonResult GetDisplayStatusJson(string jobName, string jobGroup)
		{
			var cntx = new producerinterface_Entities();
			var displayStatus = cntx.jobextend.Single(x => x.JobName == jobName).DisplayStatus;
			return Json(new
			{
				status = displayStatus,
				url = Url.Action("DisplayReport", "Report", new { jobName = jobName, jobGroup = jobGroup })
			});
		}

	}
}