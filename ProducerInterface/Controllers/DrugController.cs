using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ProducerInterfaceCommon.CatalogModels;
using System.Collections.Generic;

namespace ProducerInterface.Controllers
{
	public class DrugController : MasterBaseController
	{
		protected long userId;
		protected long producerId;
		protected catalogsEntities ccntx;

		private string[] descrFieldNames = new string[] { "Name", "EnglishName", "Description", "Interaction", "SideEffect", "IndicationsForUse", "Dosing", "Warnings", "ProductForm", "PharmacologicalAction", "Storage", "Expiration", "Composition" };

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			ccntx = new catalogsEntities();

			if (CurrentUser != null)
			{
				userId = CurrentUser.Id;
				if (CurrentUser.AccountCompany.ProducerId.HasValue)
				{
					producerId = CurrentUser.AccountCompany.ProducerId.Value;
				}
				else
				{
					ErrorMessage("Доступ в раздел Наша продукция закрыт, так как вы не представляете кого-либо из производителей");
					filterContext.Result = Redirect("~");
				}
			}
		}

		public ActionResult Index()
		{
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;

			var catalogIds = ccntx.assortment.Where(x => x.ProducerId == producerId).Select(x => x.CatalogId).ToList();
			var catalogNamesIds = ccntx.Catalog.Where(x => catalogIds.Contains(x.Id)).Select(x => x.NameId).Distinct().ToList();
			var model = ccntx.catalognames.Where(x => catalogNamesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();

			var lastChange = DB.CatalogLog
										.Where(x => catalogNamesIds.Contains(x.NameId) && x.Apply == (sbyte)ApplyRedaction.Applied)
										.GroupBy(x => x.NameId)
										.Select(grp => new { grp.Key, LastChange = grp.Max(y => y.LogTime) })
										.ToDictionary(x => x.Key, x => x.LastChange);
			ViewData["lastChange"] = lastChange;

			return View(model);
		}

		/// <summary>
		/// Возвращает историю правок препарата
		/// </summary>
		/// <param name="id">идентификатор по таблице catalogs.catalognames</param>
		/// <returns></returns>
		public ActionResult History(long id)
		{
			ViewData["Name"] = ccntx.catalognames.Single(x => x.Id == id).Name;
			var model = DB.cataloglogui.Where(x => x.NameId == id && x.Apply == (sbyte)ApplyRedaction.Applied).OrderByDescending(x => x.LogTime).ToList();
			var modelUi = MapListToUi(model);
			return View(modelUi);
		}

		/// <summary>
		/// Страница описания препарата
		/// </summary>
		/// <param name="id">идентификатор по таблице catalogs.catalognames</param>
		/// <returns></returns>
		public ActionResult EditDescription(long id)
		{
			var drugfamily = GetDrugFamilyWithCheck(id);
			if (drugfamily == null)
			{
				ErrorMessage("Препарат не найден в ассортименте производителя");
				return RedirectToAction("Index");
			}

			var mnn = ccntx.mnn.SingleOrDefault(x => x.Id == drugfamily.MnnId);
			if (mnn == null)
				mnn = new mnn();

			ViewData["familyName"] = drugfamily.Name;
			ViewData["familyId"] = id;
			ViewData["mnn"] = mnn;

			var model = ccntx.Descriptions.SingleOrDefault(x => x.Id == drugfamily.DescriptionId);
			if (model == null)
				model = new Descriptions();

			if (drugfamily.DescriptionId != null) {
				// вытащили инфу из таблицы премодерации (таблица CatalogLog базы producerinterface)
				var newLog = GetNewLog(drugfamily.DescriptionId.Value);
				ViewData["log"] = newLog;
			}

			return View(model);
		}

		private List<LogItem> GetNewLog(long descriptionId)
		{
			var result = new List<LogItem>();
			var logItems = DB.cataloglogui.Where(x => x.Type == (int)CatalogLogType.Descriptions && x.ObjectReference == descriptionId && x.Apply == (sbyte)ApplyRedaction.Applied).ToList();
			if (!logItems.Any())
				return result;

			foreach (var pn in descrFieldNames) {
				var lastItem = logItems.Where(x => x.PropertyName == pn).OrderByDescending(x => x.LogTime).FirstOrDefault();
				if (lastItem != null) {
					var logItem = new LogItem() { OperationEnum = Operation.Update, PropertyName = pn, LogTime = lastItem.LogTime, OperatorHost = lastItem.OperatorHost, OperatorName = lastItem.UserName, OperatorLogin = lastItem.Login };
					result.Add(logItem);
				}
			}
			return result;
		}

		public ActionResult DisplayForms(long id, bool edit = false)
		{
			var drugfamily = GetDrugFamilyWithCheck(id);
			if (drugfamily == null) {
				ErrorMessage("Препарат не найден в ассортименте производителя");
				return RedirectToAction("Index");
			}

			ViewData["familyName"] = drugfamily.Name;
			ViewData["familyId"] = id;
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;
			// формы данного лек.средства из ассортимента данного производителя
			var model = ccntx.Catalog.Where(x => x.NameId == id && !x.Hidden && x.assortment.Any(y => y.ProducerId == producerId)).ToList().OrderBy(x => x.Name);
			if (edit)
				return View("EditForms", model);
			return View(model);
		}

		/// <summary>
		/// Отправляет запрос на правку ПКУ
		/// </summary>
		/// <param name="familyId">Идентификатор по таблице catalognames</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult EditForms(long familyId)
		{
			var drugfamily = GetDrugFamilyWithCheck(familyId);
			if (drugfamily == null)
			{
				ErrorMessage("Препарат не найден в ассортименте производителя");
				return RedirectToAction("Index");
			}

			ViewData["familyName"] = drugfamily.Name;
			ViewData["familyId"] = familyId;
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;

			// формы данного лек.средства из ассортимента данного производителя
			var model = ccntx.Catalog.Where(x => x.NameId == familyId && !x.Hidden && x.assortment.Any(y => y.ProducerId == producerId)).ToList().OrderBy(x => x.Name);
			var ufName = new string[] { "вЫкл", "вкл" };
			var regex = new Regex(@"(?<catalogId>\d+)_(?<field>\w+)", RegexOptions.IgnoreCase);
			// форма возвращает значения для всех элементов. Ищем, что изменилось
			for (int i = 0; i < Request.Form.Count; i++) { 
				var name = Request.Form.GetKey(i);
        if (regex.IsMatch(name)) {
	        var catalogId = long.Parse(regex.Match(name).Groups["catalogId"].Value);
					var field = regex.Match(name).Groups["field"].Value;
	        var after = bool.Parse(Request.Form.GetValues(i)[0]);
					// одна строка из каталога, модель - коллекция строк
					var row = model.Single(x => x.Id == catalogId);
	        var type = row.GetType();
					if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
						type = type.BaseType;

					var propertyInfo = type.GetProperty(field);
	        var before = (bool)propertyInfo.GetValue(row);
					if (after != before) {
						var displayName = AttributeHelper.GetDisplayName(type, propertyInfo.Name);
						propertyInfo.SetValue(row, after);
						// пишем в лог для премодерации
						var dl = new CatalogLog()
						{
							After = after.ToString(),
							Before = before.ToString(),
							ObjectReference = row.Id,
							ObjectReferenceNameUi = row.Name,
							Type = (int)CatalogLogType.PKU,
							LogTime = DateTime.Now,
							OperatorHost = CurrentUser.IP,
							UserId = CurrentUser.ID_LOG,
							PropertyName = propertyInfo.Name,
							PropertyNameUi = displayName,
							NameId = familyId
						};
						DB.CatalogLog.Add(dl);
						DB.SaveChanges();

						EmailSender.SendCatalogChangeMessage(DB, CurrentUser, displayName, row.Name, ufName[Convert.ToInt32(before)], ufName[Convert.ToInt32(after)]);
					}
				}
			}
			return View("DisplayForms", model);
		}

		/// <summary>
		/// Отправляет запрос на правку описания препарата
		/// </summary>
		/// <param name="familyId">идентификатор по таблице catalognames</param>
		/// <param name="field">имя поля</param>
		/// <param name="value">значение поля</param>
		/// <returns></returns>
		public JsonResult EditDescriptionField(long familyId, string field, string value)
		{
			var drugfamily = ccntx.catalognames.Single(x => x.Id == familyId);
			var model = ccntx.Descriptions.SingleOrDefault(x => x.Id == drugfamily.DescriptionId);
			// если описания нет - создаем его
			if (model == null) {
				model = new Descriptions() { Name = drugfamily.Name };
	      ccntx.Descriptions.Add(model);
				ccntx.SaveChanges();
				drugfamily.DescriptionId = model.Id;
				ccntx.SaveChanges();
			}

			var type = model.GetType();
			if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
				type = type.BaseType;
			var propertyInfo = type.GetProperty(field);
			var before = (string)propertyInfo.GetValue(model);
			var displayName = AttributeHelper.GetDisplayName(type, propertyInfo.Name);

			// пишем в лог для премодерации
			var dl = new CatalogLog() {
				After = value,
				Before = before,
				ObjectReference = model.Id,
				ObjectReferenceNameUi = drugfamily.Name,
				Type = (int)CatalogLogType.Descriptions,
				LogTime = DateTime.Now,
				OperatorHost = CurrentUser.IP,
				UserId = CurrentUser.ID_LOG,
				PropertyName = propertyInfo.Name,
				PropertyNameUi = displayName,
				NameId = familyId
			};
			DB.CatalogLog.Add(dl);
			DB.SaveChanges();

			EmailSender.SendDescriptionChangeMessage(DB, CurrentUser, displayName, drugfamily.Name, before, value);
      return Json(new { field = field, value = value });
		}

		public JsonResult GetMnn(string term)
		{
			var ret = Json(
				ccntx.mnn.Where(x => x.Mnn1.Contains(term))
					.Take(10).ToList()
					.Select(x => new { value = x.Id, text = x.Mnn1 })
				, JsonRequestBehavior.AllowGet);
			return ret;
		}

		/// <summary>
		/// Отправляет запрос на изменение МНН
		/// </summary>
		/// <param name="familyId">идентификатор по таблице catalognames</param>
		/// <param name="mnnId">идентификатор по таблице mnn</param>
		/// <returns></returns>
		public JsonResult EditMnnField(long familyId, long mnnId)
		{
			var df = ccntx.catalognames.Single(x => x.Id == familyId);
			var before = ccntx.mnn.SingleOrDefault(x => x.Id == df.MnnId);
			var after = ccntx.mnn.SingleOrDefault(x => x.Id == mnnId);

			if (before == after || after == null)
				return Json(new { field = "Mnn1" }); ;

			var type = df.GetType();
			if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
				type = type.BaseType;

			// пишем в лог для премодерации
			var dl = new CatalogLog()
			{
				After = after?.Id.ToString(),
				Before = before?.Id.ToString(),
				ObjectReference = df.Id,
				ObjectReferenceNameUi = df.Name,
				Type = (int)CatalogLogType.MNN,
				LogTime = DateTime.Now,
				OperatorHost = CurrentUser.IP,
				UserId = CurrentUser.ID_LOG,
				PropertyName = "MnnId",
				PropertyNameUi = "МНН",
				NameId = familyId
			};
			DB.CatalogLog.Add(dl);
			DB.SaveChanges();

			EmailSender.SendMnnChangeMessage(DB, CurrentUser, df.Name, before?.Mnn1, after.Mnn1);
			return Json(new { field = "Mnn1", value = after.Mnn1 });
		}

		private ProducerInterfaceCommon.CatalogModels.catalognames GetDrugFamilyWithCheck(long id)
		{
			// идентификаторы товаров данного производителя без учёта формы выпуска
			var fmilyIds = ccntx.Catalog.Where(x => !x.Hidden && x.assortment.Any(y => y.ProducerId == producerId)).Select(x => x.NameId).Distinct().ToList();
			var drugfamily = ccntx.catalognames.SingleOrDefault(x => x.Id == id && fmilyIds.Contains(id));
			return drugfamily;
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

		private string UserFrendlyName(bool? val)
		{
			var res = "";
			if (val.HasValue && val.Value)
				res = "вкл";
			else if (val.HasValue && !val.Value)
				res = "вЫкл";
			return res;
		}


		private List<CataloglogUiPlus> MapListToUi(List<cataloglogui> model)
		{
			if (model == null)
				return null;

			var mapper = new MyAutoMapper<CataloglogUiPlus>();
			var modelUi = model.Select(x => mapper.Map(x)).ToList();
			foreach (var item in modelUi)
			{
				switch (item.TypeEnum)
				{
					case CatalogLogType.MNN:
						item.AfterUi = item.After != null ? GetMnnName(Int64.Parse(item.After)) : "";
						item.BeforeUi = item.Before != null ? GetMnnName(Int64.Parse(item.Before)) : "";
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

		private string GetMnnName(long id)
		{
			return ccntx.mnn.Find(id).Mnn1;
		}

	}
}