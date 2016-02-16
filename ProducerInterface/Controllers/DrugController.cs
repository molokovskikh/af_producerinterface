using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using ProducerInterfaceCommon.CatalogModels;
using ProducerInterfaceCommon.Models;
using System.Diagnostics;

namespace ProducerInterface.Controllers
{
	public class DrugController : MasterBaseController
	{
		//protected reportData cntx;
		protected NamesHelper h;
		protected long userId;
		protected long producerId;
		protected catalogsEntities ccntx;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			ccntx = new catalogsEntities();

			//cntx = new reportData();
			//  TODO: берётся у юзера            
			try {
				userId = CurrentUser.Id;
				producerId = (long)CurrentUser.AccountCompany.ProducerId;
			}
			catch {
				// Ignore
			}
			//h = new NamesHelper(cntx, userId);
		}

		public ActionResult Index()
		{
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;

			var catalogIds = ccntx.assortment.Where(x => x.ProducerId == producerId).Select(x => x.CatalogId).ToList();
			var model = ccntx.catalognames.Where(x => x.Catalog.Any(y => catalogIds.Contains(y.Id))).OrderBy(x => x.Name).ToList();


			//var model = cntx_.drugfamily
			//	.Where(x => x.ProducerId == producerId)
			//	.OrderBy(x => x.FamilyName)
			//	.ToList();

			return View(model);
		}

		public ActionResult EditDescription(long id)
		{
			var drugfamily = cntx_.drugfamily.SingleOrDefault(x => x.FamilyId == id && x.ProducerId == producerId);
			if (drugfamily == null)
				return View();

			ViewData["familyName"] = drugfamily.FamilyName;
			ViewData["familyId"] = id;
			ViewData["mnn"] = ccntx.mnn.SingleOrDefault(x => x.Id == drugfamily.MnnId);

			//var model = new MnnDescrComposite();

			var model = ccntx.Descriptions.SingleOrDefault(x => x.Id == drugfamily.DescriptionId);
			if (model == null)
				model = new Descriptions();
			//model.Descriptions = d;
			//model.Mnn = m;
      return View(model);
		}

		//public ActionResult EditMnn(long id)
		//{
		//	var df = cntx_.drugfamily.Single(x => x.FamilyId == id && x.ProducerId == producerId);
		//	ViewData["familyName"] = df.FamilyName;
		//	ViewData["familyId"] = id;
		//	var model = cntx_.drugmnn.SingleOrDefault(x => x.MnnId == df.MnnId);
		//	if (model == null) {
		//		model = new drugmnn();
		//	}
		//	return View(model);
		//}

		public ActionResult DisplayForms(long id, bool edit = false)
		{
			// есть ли данное семейство у данного производителя
			var drugfamily = cntx_.drugfamily.SingleOrDefault(x => x.FamilyId == id && x.ProducerId == producerId);
			if (drugfamily == null)
				return View();

      ViewData["familyName"] = drugfamily.FamilyName;
			ViewData["familyId"] = id;
			ViewData["producerName"] = ccntx.Producers.Single(x => x.Id == producerId).Name;
			// формы данного лек.средства из ассортимента данного производителя
			var model = ccntx.Catalog.Where(x => x.NameId == id && !x.Hidden && x.assortment.Any(y => y.ProducerId == producerId)).ToList().OrderBy(x => x.Name);

			//var model = cntx_.drugformproducer.Where(x => x.DrugFamilyId == id && x.ProducerId == producerId).OrderBy(x => x.CatalogName).ToList();
			if (edit)
				return View("EditForms", model);
			return View(model);
		}

		[HttpPost]
		public ActionResult EditForms(long familyId)
		{
			// есть ли данное семейство у данного производителя
			var drugfamily = cntx_.drugfamily.SingleOrDefault(x => x.FamilyId == familyId && x.ProducerId == producerId);
			if (drugfamily == null)
				return View();

			ViewData["familyName"] = drugfamily.FamilyName;
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
	        var row = model.Single(x => x.Id == catalogId);
					var propertyInfo = row.GetType().GetProperty(field);
	        var oldValue = (bool)propertyInfo.GetValue(row);
					// если изменилось TODO где-то сохранять
					if (newValue != oldValue) {
						propertyInfo.SetValue(row, newValue);
						ccntx.SaveChanges(CurrentUser, "Редактирование лек. форм");
						//var value = newValue;
					}
        }
			}
			return View(model);
			//return null;
		}

		public JsonResult EditDescriptionField(long familyId, string field, string value)
		{
			var df = ccntx.catalognames.Single(x => x.Id == familyId);
			Descriptions model;
      if (df.DescriptionId == null) {
				model = new Descriptions();
	      ccntx.Descriptions.Add(model);
				ccntx.SaveChanges(CurrentUser, "Добавление нового описания препарата");
	      df.DescriptionId = model.Id;
				ccntx.SaveChanges(CurrentUser, "Добавление ссылки на описание препарата");
			}
			else
				model = ccntx.Descriptions.Single(x => x.Id == df.DescriptionId);

			var propertyInfo = model.GetType().GetProperty(field);
			var before = (string)propertyInfo.GetValue(model);
			propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			// TODO где-то сохранять
			ccntx.SaveChanges(CurrentUser, "Изменение описания препарата");
			EmailSender.SendCatalogChangeMessage(cntx_, CurrentUser, propertyInfo.Name, df.DescriptionId, before, value);
      return Json(new { field = field, value = value });
		}

		public JsonResult GetMnn(string term)
		{
			var ret = Json(ccntx.mnn.Where(x => x.Mnn1.Contains(term)).Take(10).ToList().Select(x => new { value = x.Id, text = x.Mnn1 }), JsonRequestBehavior.AllowGet);
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