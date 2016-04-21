using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.LoggerModels;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class LogUserChangeController : MasterBaseController
	{

		private Logger_Entities cntx__ = new Logger_Entities();

		// GET: LogUserChange
		public ActionResult Index(int Id = 0)
		{
			var itemsCount = cntx__.logchangeview.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("LogItemsPerPage"));
			var info = new SortingPagingInfo() { CurrentPageIndex = Id, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };

			if (info.PageCount < Id && Id != 0)
				return RedirectToAction("Index", new { Id = 0 });

			ViewBag.Info = info;

			var model = cntx__.logchangeview.OrderByDescending(xxx => xxx.ChangeSetId).Skip(itemsPerPage * Id).Take(itemsPerPage).ToList();
			return View(model);
		}

		public ActionResult ReadMore(int Id)
		{

			if (Id == 0)
				return RedirectToAction("Index");

			var model = cntx__.propertychangeview.Where(x => x.ChangeObjectId == Id).ToList();
			return View(model);
		}
	}
}