using ProducerInterfaceCommon.Heap;
using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace ProducerInterface.Controllers
{
	public class DrugController : MasterBaseController
	{
		//protected reportData cntx;
		protected NamesHelper h;
		protected long userId;
		protected long producerId;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			//cntx = new reportData();
			//  TODO: берётся у юзера            
			try {
				userId = CurrentUser.Id;
				producerId = (long)CurrentUser.ProducerId;
			}
			catch {
				// Ignore
			}
			//h = new NamesHelper(cntx, userId);
		}

		public ActionResult Index()
		{
			//var asdf = cntx_.DrugDescriptionRemark.Single(x => x.Id == 3);
			//asdf.Description = "Правка11NEW";
			//var asdf = new DrugDescriptionRemark() { Description = "Правка 6" };
			//cntx_.DrugDescriptionRemark.Add(asdf);
			////cntx_.DrugDescriptionRemark.Remove(asdf);
			//cntx_.SaveChanges(CurrentUser);
			//var asdf = cntx_.DrugDescriptionRemark.Single(x => x.Id == 2);
			//asdf.Description = "Правка6";
			//var asdf = new DrugDescriptionRemark() { Description = "Правка 5" };
			//cntx_.DrugDescriptionRemark.Add(asdf);
			//cntx_.SaveChanges();

			ViewData["producerName"] = cntx_.producernames.Single(x => x.ProducerId == producerId).ProducerName;
      var model = cntx_.drugfamily
				.Where(x => x.ProducerId == producerId)
				.OrderBy(x => x.FamilyName)
				.ToList();
			return View(model);
		}

		public ActionResult EditDescription(long id)
		{
			var df = cntx_.drugfamily.Single(x => x.FamilyId == id && x.ProducerId == producerId);
			ViewData["familyName"] = df.FamilyName;
			ViewData["familyId"] = id;
			ViewData["add"] = false;
			var model = cntx_.drugdescription.SingleOrDefault(x => x.DescriptionId == df.DescriptionId);
			if (model == null) {
				ViewData["add"] = true;
				model = new drugdescription();
			}
			return View(model);
		}

		public ActionResult EditMnn(long id)
		{
			var df = cntx_.drugfamily.Single(x => x.FamilyId == id && x.ProducerId == producerId);
			ViewData["familyName"] = df.FamilyName;
			ViewData["familyId"] = id;
			ViewData["add"] = false;
			var model = cntx_.drugmnn.SingleOrDefault(x => x.MnnId == df.MnnId);
			if (model == null) {
				ViewData["add"] = true;
				model = new drugmnn();
			}
			return View(model);
		}

		public ActionResult DisplayForms(long id, bool edit = false)
		{
			var familyName = cntx_.drugfamily.Single(x => x.FamilyId == id && x.ProducerId == producerId).FamilyName;
			ViewData["familyName"] = familyName;
			ViewData["familyId"] = id;
			// формы данного лек.средства из ассортимента данного производителя
			var model = cntx_.drugformproducer.Where(x => x.DrugFamilyId == id && x.ProducerId == producerId).OrderBy(x => x.CatalogName).ToList();
			if (edit)
				return View("EditForms", model);
			return View(model);
		}

		[HttpPost]
		public ActionResult EditForms(long familyId)
		{
			var familyName = cntx_.drugfamily.Single(x => x.FamilyId == familyId && x.ProducerId == producerId).FamilyName;
			ViewData["familyName"] = familyName;
			ViewData["familyId"] = familyId;
			// формы данного лек.средства из ассортимента данного производителя
			var model = cntx_.drugformproducer.Where(x => x.DrugFamilyId == familyId && x.ProducerId == producerId).OrderBy(x => x.CatalogName).ToList();

			var regex = new Regex(@"(?<catalogId>\d+)_(?<field>\w+)", RegexOptions.IgnoreCase);
			// форма возвращает значения для всех элементов. Ищем, что изменилось
			for (int i = 0; i < Request.Form.Count; i++) { 
				var name = Request.Form.GetKey(i);
        if (regex.IsMatch(name)) {
	        var catalogId = long.Parse(regex.Match(name).Groups["catalogId"].Value);
					var field = regex.Match(name).Groups["field"].Value;
	        var newValue = bool.Parse(Request.Form.GetValues(i)[0]);
	        var row = model.Single(x => x.CatalogId == catalogId);
					var propertyInfo = row.GetType().GetProperty(field);
	        var oldValue = (bool)propertyInfo.GetValue(row);
					// если изменилось TODO где-то сохранять
					if (newValue != oldValue) {
						propertyInfo.SetValue(row, newValue);
						var value = newValue;
	        }
        }
			}
			return View(model);
			//return null;
		}

		public JsonResult EditDescriptionField(long familyId, string field, string value)
		{
			var df = cntx_.drugfamily.Single(x => x.FamilyId == familyId && x.ProducerId == producerId);
			drugdescription model;
      if (df.DescriptionId == null) {
				model = new drugdescription();
	      cntx_.drugdescription.Add(model);
				//cntx_.SaveChanges();
	      df.DescriptionId = model.DescriptionId;
				//cntx_.SaveChanges();
			}
			else
				model = cntx_.drugdescription.Single(x => x.DescriptionId == df.DescriptionId);

			var propertyInfo = model.GetType().GetProperty(field);
			propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			// TODO где-то сохранять
			//cntx_.SaveChanges();
			return Json(new { field = field, value = value });
		}

		public JsonResult EditMnnField(long familyId, string field, string value)
		{
			var df = cntx_.drugfamily.Single(x => x.FamilyId == familyId && x.ProducerId == producerId);
			drugmnn model;
			if (df.MnnId == null)
			{
				model = new drugmnn();
				cntx_.drugmnn.Add(model);
				//cntx_.SaveChanges();
				df.MnnId = model.MnnId;
				//cntx_.SaveChanges();
			}
			else
				model = cntx_.drugmnn.Single(x => x.MnnId == df.MnnId);
			
			var propertyInfo = model.GetType().GetProperty(field);
			propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			// TODO где-то сохранять
			//cntx_.SaveChanges();
			return Json(new { field = field, value = value });
		}


		//[HttpPost]
		//public ActionResult CreateDrugDescriptionRemark(int id, [EntityBinder] DrugDescriptionRemark drugDescriptionRemark)
		//{
		//    var user = GetCurrentUser();
		//    var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
		//    drugDescriptionRemark.ProducerUser = user;
		//    drugDescriptionRemark.DrugFamily = family;
		//    var errors = ValidationRunner.Validate(drugDescriptionRemark);
		//    if (errors.Length == 0)
		//    {
		//        DbSession.Save(drugDescriptionRemark);
		//        SuccessMessage("Запрос на изменение описания отправлен модератору.");
		//        return Redirect(GetIndexActionUrl());
		//    }
		//    ErrorMessage("Произошла ошибка.");
		//    CreateDrugDescriptionRemark(id);
		//    ViewBag.DrugDescriptionRemark = drugDescriptionRemark;
		//    return View("CreateDrugDescriptionRemark");
		//}
	}
}