using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ProducerInterfaceCommon.CatalogModels;
using ProducerInterfaceCommon.LogsModels;


namespace ProducerInterface.Controllers
{
	public class DrugController : MasterBaseController
	{
		protected long userId;
		protected long producerId;
		protected catalogsEntities ccntx;
		protected LogsEntities lcntx;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			ccntx = new catalogsEntities();
			lcntx = new LogsEntities();

			if (CurrentUser != null)
			{
				userId = CurrentUser.Id;
				producerId = (long)CurrentUser.AccountCompany.ProducerId;
			}
		}

		public ActionResult Index()
		{
			//var asdsd = lcntx.CatalogLogs.Where(x => x.CatalogId == 30346).Select(x => x.Id);


			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;

			var catalogIds = ccntx.assortment.Where(x => x.ProducerId == producerId).Select(x => x.CatalogId).ToList();
			var catalogNamesIds = ccntx.Catalog.Where(x => catalogIds.Contains(x.Id)).Select(x => x.NameId).Distinct().ToList();
			var model = cntx_.catalognameswithuptime.Where(x => catalogNamesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();

			return View(model);
		}

		// Страница описания препарата (id по таблице catalogs.catalognames)
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
      return View(model);
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
							PropertyNameUi = displayName
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
				PropertyNameUi = displayName
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
				PropertyNameUi = "МНН"
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
	}
}