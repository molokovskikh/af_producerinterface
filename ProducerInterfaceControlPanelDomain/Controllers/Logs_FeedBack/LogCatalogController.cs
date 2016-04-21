using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.CatalogModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class LogCatalogController : MasterBaseController
	{
		protected catalogsEntities ccntx;

		protected Dictionary<long, string> mnnNames;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ccntx = new catalogsEntities();
			mnnNames = ccntx.mnn.ToDictionary(x => x.Id, x => x.Mnn1);
		}

		// GET: LogUserChange
		public ActionResult Index(int Id = 0)
		{
			var itemsCount = cntx_.cataloglogui.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("LogItemsPerPage"));

			var info = new SortingPagingInfo() { CurrentPageIndex = Id, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };
			ViewBag.Info = info;

			// если требуется страница, которой нет
			if (info.PageCount < Id && Id != 0)
				return RedirectToAction("Index", new { Id = 0 });

			var model = cntx_.cataloglogui.OrderByDescending(x => x.Id).Skip(itemsPerPage * Id).Take(itemsPerPage).ToList();
			var modelUi = MapListToUi(model);
			return View(modelUi);
		}

		public ActionResult ReadMore(int Id)
		{

			if (Id == 0)
				return RedirectToAction("Index");

			var model = cntx_.cataloglogui.Where(x => x.Id == Id).ToList();

			return View(model);
		}

		private string UserFrendlyName(string val)
		{
			var res = "вЫкл";
			if (val == "True")
				res = "вкл";
			return res;
		}

		private List<CataloglogUiPlus> MapListToUi(List<cataloglogui> model)
		{
			if (model == null)
				return null;

			var mapper = new MyAutoMapper<CataloglogUiPlus>();
			var modelUi = model.Select(x => mapper.Map(x)).ToList();
			foreach (var item in modelUi) {
				switch (item.TypeEnum) {
					case CatalogLogType.MNN:
						item.AfterUi = item.After !=  null ? mnnNames[Int64.Parse(item.After)] : "";
						item.BeforeUi = item.Before != null ? mnnNames[Int64.Parse(item.Before)] : "";
						break;
					case CatalogLogType.PKU:
						item.AfterUi = UserFrendlyName(item.After);
						item.BeforeUi = UserFrendlyName(item.Before);
						break;
					default:
						item.AfterUi = item.After;
						item.BeforeUi = item.Before;
						break;
				}
			}

			return modelUi;
		}
	}
}