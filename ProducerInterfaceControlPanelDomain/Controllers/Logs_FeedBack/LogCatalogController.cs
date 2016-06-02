using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.CatalogModels;
using System.Web.Caching;
using System.Web.Mvc.Html;

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

		public ActionResult Index(CatalogLogFilter model)
		{
			if (model.ApplyId.HasValue) {
				Apply(model.ApplyId.Value);
				SuccessMessage("Правки успешно внесены в каталог");
			}
			if (model.RejectId.HasValue) {
				Reject(model.RejectId.Value, model.RejectComment);
				SuccessMessage("Комментарий отправлен пользователю");
			}
			var applyList = new List<SelectListItem>();
			var emptyItem = new SelectListItem() { Value = "", Text = "Все правки" };
			applyList.Add(emptyItem);
			if (model.Apply.HasValue)
				applyList.AddRange(EnumHelper.GetSelectList(typeof(ApplyRedaction), (ApplyRedaction)model.Apply));
			else
				applyList.AddRange(EnumHelper.GetSelectList(typeof(ApplyRedaction)));
			model.ApplyList = applyList;
			return View(model);
		}

		public ActionResult SearchResult(CatalogLogFilter filter)
		{
			var query = cntx_.cataloglogui.AsQueryable();
			if (filter.Apply.HasValue)
				query = query.Where(x => x.Apply == filter.Apply);

			var itemsCount = query.Count();
			var itemsPerPage = Convert.ToInt32(GetWebConfigParameters("ReportCountPage"));
			var info = new SortingPagingInfo() { CurrentPageIndex = filter.CurrentPageIndex, ItemsCount = itemsCount, ItemsPerPage = itemsPerPage };
			ViewBag.Info = info;

			var model = query.OrderByDescending(x => x.Id).Skip(itemsPerPage * filter.CurrentPageIndex).Take(itemsPerPage).ToList();
			var modelUi = MapListToUi(model);
			return View(modelUi);
		}

		private void Apply(long id)
		{
			var item = cntx_.CatalogLog.Single(x => x.Id == id);
			switch (item.TypeEnum)
			{
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

			item.Apply = (sbyte)ApplyRedaction.Applied;
			item.AdminId = CurrentUser.Id;
			item.DateEdit = DateTime.Now;
			cntx_.SaveChanges();
			// SendMessage
		}

		private void Reject(long id, string comment)
		{
			var item = cntx_.CatalogLog.Single(x => x.Id == id);
			item.Apply = (sbyte)ApplyRedaction.Rejected;
			item.AdminId = CurrentUser.Id;
			item.DateEdit = DateTime.Now;
			cntx_.SaveChanges();

			var user = cntx_.Account.Single(x => x.Id == item.UserId);
			var before = item.Before;
			var after = item.After;

			switch (item.TypeEnum)
			{
				case CatalogLogType.MNN:
					after = item.After != null ? mnnNames[Int64.Parse(item.After)] : "";
					before = item.Before != null ? mnnNames[Int64.Parse(item.Before)] : "";
					break;
				case CatalogLogType.PKU:
					after = UserFrendlyName(item.After);
					before = UserFrendlyName(item.Before);
					break;
			}

			EmailSender.SendRejectCatalogChangeMessage(cntx_, user, item.ObjectReferenceNameUi, item.PropertyNameUi, before, after, comment, CurrentUser.Id);
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
			var res = "";
			if (val == "True")
				res = "вкл";
			else if (val == "False")
				res = "вЫкл";
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