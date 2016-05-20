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
				producerId = (long)CurrentUser.AccountCompany.ProducerId;
			}
		}

		public ActionResult Index()
		{
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;

			var catalogIds = ccntx.assortment.Where(x => x.ProducerId == producerId).Select(x => x.CatalogId).ToList();
			var catalogNamesIds = ccntx.Catalog.Where(x => catalogIds.Contains(x.Id)).Select(x => x.NameId).Distinct().ToList();
			var model = ccntx.catalognames.Where(x => catalogNamesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();

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
			var model = cntx_.cataloglogui.Where(x => x.NameId == id && x.Apply).OrderByDescending(x => x.LogTime).ToList();
			// если есть история в новом логе
			if (model.Any()) {
				var modelUi = MapListToUi(model);
				return View(modelUi);
			}
			// если нет - вытаскиваем из старого
			else {
				var modelC = GetOldCatalogLogList(id);
				var modelD = GetOldDescriptionLogList(id);
				modelD.AddRange(modelC);
				var modelUi = modelD.OrderByDescending(x => x.LogTime).ToList();
				return View(modelUi);
			}
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

			//var drugfamily = ccntx.catalognames.Single(x => x.Id == id);
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
				// вытащили инфу из основного лога (таблица descriptionlogs базы logs)
				var oldLog = GetOldLog(drugfamily.DescriptionId.Value);
				foreach (var logItem in newLog)
				{
					// если инфа (расширенная) по элементу есть в новом логе, удалили её дубликат из основного
					var existItem = oldLog.SingleOrDefault(x => x.PropertyName == logItem.PropertyName);
					if (existItem != null)
						oldLog.Remove(existItem);
				}
				// добавили
				newLog.AddRange(oldLog);
				ViewData["log"] = newLog;
			}

			return View(model);
		}

		private List<LogItem> GetNewLog(long descriptionId)
		{
			var result = new List<LogItem>();
			var logItems = cntx_.cataloglogui.Where(x => x.Type == (int)CatalogLogType.Descriptions && x.ObjectReference == descriptionId && x.Apply == true).ToList();
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

		private List<LogItem> GetOldLog(long descriptionId)
		{
			var o = new descriptionlogview();
			var type = o.GetType();
			var pi = type.GetProperties().Where(x => descrFieldNames.Contains(x.Name)).ToList();

			var logItems = cntx_.descriptionlogview.Where(x => x.DescriptionId == descriptionId).ToList();
			var result = new List<LogItem>();
			foreach (var p in pi)
			{
				var lastItem = logItems.Where(x => p.GetValue(x) != null).OrderByDescending(x => x.LogTime).FirstOrDefault();
				if (lastItem != null)
				{
					var logItem = new LogItem() { OperationEnum = (Operation)lastItem.Operation, PropertyName = p.Name, LogTime = lastItem.LogTime, OperatorHost = lastItem.OperatorHost, OperatorName = lastItem.OperatorName };
					result.Add(logItem);
				}
			}
			return result;
		}

		/// <summary>
		/// Возвращает записи из старого лога изменений описания, преобразуя их в новую структуру
		/// </summary>
		/// <param name="id">идентификатор по таблице catalogs.catalognames</param>
		/// <returns></returns>
		private List<CataloglogUiPlus> GetOldDescriptionLogList(long id)
		{
			var result = new List<CataloglogUiPlus>();
			var cn = ccntx.catalognames.Single(x => x.Id == id);

			if (!cn.DescriptionId.HasValue)
				return result;

			var logItems = cntx_.descriptionlogview.Where(x => x.DescriptionId == cn.DescriptionId.Value).OrderByDescending(x => x.LogTime).ToList();
			var o = new descriptionlogview();
			var type = o.GetType();
			var pi = type.GetProperties().Where(x => descrFieldNames.Contains(x.Name)).ToList();

			var meta = new Descriptions();
			var metaType = meta.GetType();

			for (int i = 0; i < logItems.Count; i++)
			{
				var itemCur = logItems[i];
				var itemPrev = o;
				if (i < logItems.Count - 1)
					itemPrev = logItems[i+1];
				foreach (var p in pi) {
					var valCur = p.GetValue(itemCur);
					if (valCur == null)
						continue;
					var valPrev = p.GetValue(itemPrev);
					if (valCur != valPrev) {
						var logItem = new CataloglogUiPlus()
						{
							Id = itemCur.Id,
							Type = (int)CatalogLogType.Descriptions,
							ObjectReferenceNameUi = cn.Name,
							BeforeUi = (string)valPrev,
							AfterUi = (string)valCur,
							PropertyNameUi = AttributeHelper.GetDisplayName(metaType, p.Name),
							LogTime = itemCur.LogTime,
							OperatorHost = itemCur.OperatorHost,
							UserName = itemCur.OperatorName,
							Login = "farm@analit.net",
							DateEdit = itemCur.LogTime
						};
						result.Add(logItem);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Возвращает записи из старого лога изменений каталога, преобразуя их в новую структуру
		/// </summary>
		/// <param name="id">идентификатор по таблице catalogs.catalognames</param>
		/// <returns></returns>
		private List<CataloglogUiPlus> GetOldCatalogLogList(long id)
		{
			var result = new List<CataloglogUiPlus>();

			var cnameDic = ccntx.Catalog.Where(x => x.NameId == id).ToDictionary(x => x.Id, x => x.Name);
			var catIds = ccntx.assortment.Where(x => x.ProducerId == producerId && x.Catalog.NameId == id).Select(x => x.CatalogId).ToList();
			var logItems = cntx_.cataloglogview.Where(x => catIds.Contains(x.CatalogId.Value)).ToList();

			var combined = logItems
				.Where(x => x.NewCombined != x.OldCombined)
				.Select(x => new CataloglogUiPlus() { Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldCombined),
					AfterUi = UserFrendlyName(x.NewCombined),
					PropertyNameUi = "Комбинированные",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(combined);

			var vitallyImportant = logItems
				.Where(x => x.NewVitallyImportant != x.OldVitallyImportant)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldVitallyImportant),
					AfterUi = UserFrendlyName(x.NewVitallyImportant),
					PropertyNameUi = "Жизненно важные",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(vitallyImportant);

			var mandatoryList = logItems
				.Where(x => x.NewMandatoryList != x.OldMandatoryList)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldMandatoryList),
					AfterUi = UserFrendlyName(x.NewMandatoryList),
					PropertyNameUi = "Обязательный ассортимент",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(mandatoryList);

			var narcotic = logItems
				.Where(x => x.NewNarcotic != x.OldNarcotic)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldNarcotic),
					AfterUi = UserFrendlyName(x.NewNarcotic),
					PropertyNameUi = "Наркотические и психотропные",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(narcotic);

			var toxic = logItems
				.Where(x => x.NewToxic != x.OldToxic)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldToxic),
					AfterUi = UserFrendlyName(x.NewToxic),
					PropertyNameUi = "Сильнодействующие и ядовитые",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(toxic);

			var other = logItems
				.Where(x => x.NewOther != x.OldOther)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldOther),
					AfterUi = UserFrendlyName(x.NewOther),
					PropertyNameUi = "Иные лекарственные средства",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(other);

			var monobrend = logItems
				.Where(x => x.NewMonobrend != x.OldMonobrend)
				.Select(x => new CataloglogUiPlus()
				{
					Id = x.Id,
					Type = (int)CatalogLogType.PKU,
					ObjectReferenceNameUi = cnameDic[x.CatalogId.Value],
					BeforeUi = UserFrendlyName(x.OldMonobrend),
					AfterUi = UserFrendlyName(x.NewMonobrend),
					PropertyNameUi = "Форма выпуска производится исключительно",
					LogTime = x.LogTime,
					OperatorHost = x.OperatorHost,
					UserName = x.OperatorName,
					Login = "farm@analit.net",
					DateEdit = x.LogTime
				}).ToList();
			result.AddRange(monobrend);

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
						cntx_.CatalogLog.Add(dl);
						cntx_.SaveChanges();

						EmailSender.SendCatalogChangeMessage(cntx_, CurrentUser, displayName, row.Name, ufName[Convert.ToInt32(before)], ufName[Convert.ToInt32(after)]);
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
			cntx_.CatalogLog.Add(dl);
			cntx_.SaveChanges();

			EmailSender.SendDescriptionChangeMessage(cntx_, CurrentUser, displayName, drugfamily.Name, before, value);
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
			cntx_.CatalogLog.Add(dl);
			cntx_.SaveChanges();

			EmailSender.SendMnnChangeMessage(cntx_, CurrentUser, df.Name, before?.Mnn1, after.Mnn1);
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