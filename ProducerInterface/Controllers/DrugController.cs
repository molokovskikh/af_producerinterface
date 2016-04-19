using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ProducerInterfaceCommon.CatalogModels;

namespace ProducerInterface.Controllers
{
	public class DrugController : MasterBaseController
	{
		protected long userId;
		protected long producerId;
		protected catalogsEntities ccntx;

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
			var model = cntx_.catalognameswithuptime.Where(x => catalogNamesIds.Contains(x.Id)).OrderBy(x => x.Name).ToList();

			return View(model);
		}

		// id по таблице catalogs.catalognames
		public ActionResult EditDescription(long id)
		{
			//var drugfamily = GetDrugFamilyWithCheck(id);
			//if (drugfamily == null) {
			//	ErrorMessage("Препарат не найден в ассортименте производителя");
			//	return RedirectToAction("Index");
			//}
			var drugfamily = ccntx.catalognames.SingleOrDefault(x => x.Id == id);

			ViewData["familyName"] = drugfamily.Name;
			ViewData["familyId"] = id;
			ViewData["mnn"] = ccntx.mnn.SingleOrDefault(x => x.Id == drugfamily.MnnId);

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

			var regex = new Regex(@"(?<catalogId>\d+)_(?<field>\w+)", RegexOptions.IgnoreCase);
			// форма возвращает значения для всех элементов. Ищем, что изменилось
			for (int i = 0; i < Request.Form.Count; i++) { 
				var name = Request.Form.GetKey(i);
        if (regex.IsMatch(name)) {
	        var catalogId = long.Parse(regex.Match(name).Groups["catalogId"].Value);
					var field = regex.Match(name).Groups["field"].Value;
	        var newValue = bool.Parse(Request.Form.GetValues(i)[0]);
					// одна строка из каталога, модель - коллекция строк
					var row = model.Single(x => x.Id == catalogId);
					var propertyInfo = row.GetType().GetProperty(field);
	        var oldValue = (bool)propertyInfo.GetValue(row);
					if (newValue != oldValue) {
						propertyInfo.SetValue(row, newValue);
						ccntx.SaveChanges(CurrentUser, "Редактирование лек. форм");
						EmailSender.SendCatalogChangeMessage(cntx_, CurrentUser, propertyInfo.Name, row.Name, oldValue.ToString(), newValue.ToString());
					}
				}
			}
			return View("DisplayForms", model);
		}

		public JsonResult EditDescriptionField(long familyId, string field, string value)
		{
			var drugfamily = ccntx.catalognames.Single(x => x.Id == familyId);
			var model = ccntx.Descriptions.SingleOrDefault(x => x.Id == drugfamily.DescriptionId);
			// если описания нет - создаем его
			if (model == null) {
				model = new Descriptions() { Name = drugfamily.Name };
	      ccntx.Descriptions.Add(model);
				ccntx.SaveChanges(CurrentUser, "Добавление нового описания препарата");
				drugfamily.DescriptionId = model.Id;
				ccntx.SaveChanges(CurrentUser, "Добавление ссылки на описание препарата");
			}

			var propertyInfo = model.GetType().GetProperty(field);
			var before = (string)propertyInfo.GetValue(model);
			
			// пишем в лог для премодерации
			var dl = new DescriptionLog() {
				After = value,
				Before = before,
				DescriptionId = model.Id,
				LogTime = DateTime.Now,
				OperatorHost = CurrentUser.IP,
				UserId = CurrentUser.ID_LOG,
				PropertyName = propertyInfo.Name
			};
			cntx_.DescriptionLog.Add(dl);
			cntx_.SaveChanges();

			//propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			//ccntx.SaveChanges(CurrentUser, "Изменение описания препарата");
			// TODO письмо в шаблоны, значения и названия полей - по-русски
			//EmailSender.SendDescriptionChangeMessage(cntx_, CurrentUser, propertyInfo.Name, drugfamily.Name, before, value);
      return Json(new { field = field, value = value });
		}

		private ProducerInterfaceCommon.CatalogModels.catalognames GetDrugFamilyWithCheck(long id)
		{
			// идентификаторы товаров данного производителя без учёта формы выпуска
			var fmilyIds = ccntx.Catalog.Where(x => !x.Hidden && x.assortment.Any(y => y.ProducerId == producerId)).Select(x => x.NameId).Distinct().ToList();
			var drugfamily = ccntx.catalognames.SingleOrDefault(x => x.Id == id && fmilyIds.Contains(id));
			return drugfamily;
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

		public JsonResult EditMnnField(long familyId, long mnnId)
		{
			var df = ccntx.catalognames.Single(x => x.Id == familyId);
			var before = ccntx.mnn.SingleOrDefault(x => x.Id == df.MnnId);
			var after = ccntx.mnn.Single(x => x.Id == mnnId);

			df.MnnId = mnnId;
      ccntx.SaveChanges(CurrentUser, "Изменение МНН");

			EmailSender.SendMnnChangeMessage(cntx_, CurrentUser, df.Name, before != null ? before.Mnn1 : "", after.Mnn1);
			return Json(new { field = "Mnn1", value = after.Mnn1 });
		}
	}
}