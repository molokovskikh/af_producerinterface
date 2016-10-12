using System;
using System.Linq;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;
using System.Collections.Generic;
using System.Configuration;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.CatalogModels;
using System.Web.Caching;
using System.Web.Mvc.Html;
using NHibernate.Linq;
using ProducerInterfaceCommon.Models;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
	public class LogCatalogController : BaseController
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
			var emptyItem = new SelectListItem() {Value = "", Text = "Все правки"};
			applyList.Add(emptyItem);
			if (model.Apply.HasValue && model.IsUserFilter)
				applyList.AddRange(EnumHelper.GetSelectList(typeof (ApplyRedaction), (ApplyRedaction) model.Apply));
			else {
				if (model.IsUserFilter) {
					applyList.AddRange(EnumHelper.GetSelectList(typeof (ApplyRedaction)));
				} else {
					var defaultValue = int.Parse(ConfigurationManager.AppSettings["FilterDefault_LogCatalogList"]);
					applyList.AddRange(EnumHelper.GetSelectList(typeof (ApplyRedaction), (ApplyRedaction) defaultValue));
				}
			}
			model.ApplyList = applyList;
			return View(model);
		}

		public ActionResult SearchResult(CatalogLogFilter filter)
		{
			var query = DB.cataloglogui.AsQueryable();
			if (filter.Apply.HasValue && filter.IsUserFilter)
				query = query.Where(x => x.Apply == filter.Apply);
			else {
				if (!filter.IsUserFilter) {
					var defaultValue = int.Parse(ConfigurationManager.AppSettings["FilterDefault_LogCatalogList"]);
					query = query.Where(x => x.Apply == defaultValue);
				}
			}
			var itemsCount = query.Count();
			var itemsPerPage = Convert.ToInt32(ConfigurationManager.AppSettings["ReportCountPage"]);
			var info = new SortingPagingInfo() {
				CurrentPageIndex = filter.CurrentPageIndex,
				ItemsCount = itemsCount,
				ItemsPerPage = itemsPerPage
			};
			ViewBag.Info = info;

			var model = query.OrderByDescending(x => x.Id).Skip(itemsPerPage*filter.CurrentPageIndex).Take(itemsPerPage).ToList();
			var modelUi = MapListToUi(model);
			return View(modelUi);
		}

		private void Apply(long id)
		{
			var item = DB.CatalogLog.Single(x => x.Id == id);
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
				case CatalogLogType.Photo:
					var photo =
						DbSession.Query<DrugFormPicture>()
							.First(x => x.CatalogId == item.ObjectReference && x.ProducerId == item.ProducerId);
					photo.PictureKey = Convert.ToInt32(item.After);
					DbSession.SaveOrUpdate(photo);
					DbSession.Transaction.Commit();
					break;
			}
			ccntx.SaveChanges();

			item.Apply = (sbyte) ApplyRedaction.Applied;
			item.AdminId = CurrentUser.Id;
			item.DateEdit = DateTime.Now;
			DB.SaveChanges();
			// SendMessage
		}

		private void Reject(long id, string comment)
		{
			var item = DB.CatalogLog.Single(x => x.Id == id);
			item.Apply = (sbyte) ApplyRedaction.Rejected;
			item.AdminId = CurrentUser.Id;
			item.DateEdit = DateTime.Now;
			DB.SaveChanges();

			var user = DB.Account.Single(x => x.Id == item.UserId);
			var before = item.Before;
			var after = item.After;

			switch (item.TypeEnum) {
				case CatalogLogType.MNN:
					after = item.After != null ? mnnNames[Int64.Parse(item.After)] : "";
					before = item.Before != null ? mnnNames[Int64.Parse(item.Before)] : "";
					break;
				case CatalogLogType.PKU:
					after = UserFrendlyName(item.After);
					before = UserFrendlyName(item.Before);
					break;
				case CatalogLogType.Photo:
					after = string.IsNullOrEmpty(item.After) ? "Изображение отсутствует" : "Новое изображение";
					before = string.IsNullOrEmpty(item.Before) ? "Изображение отсутствует" : "Текущее изображение";
					break;
			}

			EmailSender.SendRejectCatalogChangeMessage(DB, user, item.ObjectReferenceNameUi, item.PropertyNameUi, before, after,
				comment);
		}


		/// <summary>
		/// Возвращает историю правок препарата
		/// </summary>
		/// <param name="id">идентификатор по таблице catalogs.catalognames</param>
		/// <returns></returns>
		public ActionResult Changes(long id)
		{
			ViewData["Name"] = ccntx.catalognames.Single(x => x.Id == id).Name;
			var model =
				DB.cataloglogui.Where(x => x.NameId == id && x.Apply == (sbyte) ApplyRedaction.Applied)
					.OrderByDescending(x => x.LogTime)
					.ToList();
			var modelUi = MapListToUi(model);
			return View(modelUi);
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
			if (uType != null && string.IsNullOrEmpty(value)) {
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
				res = "да";
			else if (val == "False")
				res = "нет";
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
						item.AfterUi = item.After != null ? mnnNames[Int64.Parse(item.After)] : "";
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