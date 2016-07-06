using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class LogForNetController : MasterBaseController
	{
		// GET: LogForNet
		public string Log { get; set; }

		public ActionResult Index(int Id = 0)
		{
			var itemsCount = DB.LogForNet.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("ErrorCountPage"));
			var info = new SortingPagingInfo() { CurrentPageIndex = Id, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };

			if (info.PageCount < Id && Id != 0)
				RedirectToAction("Index");

			ViewBag.Info = info;
			var model = DB.LogForNet.OrderByDescending(xxx => xxx.Id).Skip(Id * itemsPerPage).Take(itemsPerPage).ToList();

			return View(model);
		}
	}
}