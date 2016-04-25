using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.CatalogModels;
using System.Web.Caching;

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

			// кешируем МНН
			var key = "mnnNames";
			mnnNames = HttpContext.Cache.Get(key) as Dictionary<long, string> ?? ccntx.mnn.ToDictionary(x => x.Id, x => x.Mnn1);
			HttpContext.Cache.Insert(key, mnnNames, null, DateTime.UtcNow.AddSeconds(300), Cache.NoSlidingExpiration);
		}

		[HttpGet]
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

		[HttpPost]
		public ActionResult Index(int Id, List<long> apply)
		{
			var items = cntx_.CatalogLog.Where(x => apply.Contains(x.Id)).ToList();
			foreach (var item in items) {
				switch (item.TypeEnum) {
						case CatalogLogType.Descriptions:
							var description = ccntx.Descriptions.Single(x => x.Id == item.ObjectReference);
							SetValue(description, item.PropertyName, item.After);
							break;
						case CatalogLogType.MNN:
							var drugfamily = ccntx.catalognames.Single(x => x.Id == item.ObjectReference);
							SetValue(drugfamily, item.PropertyName, item.After);
						break;
						case CatalogLogType.PKU:
							var catalog = ccntx.Catalog.Single(x => x.Id == item.ObjectReference);
							SetValue(catalog, item.PropertyName, item.After);
							break;
				}
				ccntx.SaveChanges();
				item.Apply = true;
			}

			cntx_.SaveChanges();
			SuccessMessage("Правки успешно внесены в каталог");
			return RedirectToAction("Index", new { Id = Id });
		}

		private void SetValue(object o, string propName, string value)
		{
			var type = o.GetType();
			var pi = type.GetProperties();
			var p = pi.SingleOrDefault(x => x.Name == propName);
			if (p == null)
				return;

			var uType = Nullable.GetUnderlyingType(p.PropertyType);
			// если тип допускает null и пришёл null - ставим null
			if (uType != null && string.IsNullOrEmpty(value))
			{
				p.SetValue(o, null);
				return;
			}
			// если тип не допускает null и пришёл null
			if (uType == null && string.IsNullOrEmpty(value))
				throw new NotSupportedException("Попытка записи null в поле, не допускающее null");

			// если пришёл не null
			var castValue = Convert.ChangeType(value, uType ?? p.PropertyType);
			p.SetValue(o, castValue);
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