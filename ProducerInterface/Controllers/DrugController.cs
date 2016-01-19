﻿using Quartz.Job;
using Quartz.Job.EDM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;
using System.Text.RegularExpressions;

namespace ProducerInterface.Controllers
{
	public class DrugController : pruducercontroller.BaseController
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
			ViewData["producerName"] = _BD_.producernames.Single(x => x.ProducerId == producerId).ProducerName;
			var model = _BD_.drugfamily
				.Where(x => x.ProducerId == producerId)
				.OrderBy(x => x.FamilyName)
				.ToList();
			return View(model);
		}

		public ActionResult EditDescription(long id)
		{
			var df = _BD_.drugfamily.Single(x => x.FamilyId == id);
			ViewData["familyName"] = df.FamilyName;
			ViewData["familyId"] = id;
			ViewData["add"] = false;
			var model = _BD_.drugdescription.SingleOrDefault(x => x.DescriptionId == df.DescriptionId);
			if (model == null) {
				ViewData["add"] = true;
				model = new drugdescription();
			}
			return View(model);
		}

		public ActionResult EditMnn(long id)
		{
			var df = _BD_.drugfamily.Single(x => x.FamilyId == id);
			ViewData["familyName"] = df.FamilyName;
			ViewData["familyId"] = id;
			ViewData["add"] = false;
			var model = _BD_.drugmnn.SingleOrDefault(x => x.MnnId == df.MnnId);
			if (model == null) {
				ViewData["add"] = true;
				model = new drugmnn();
			}
			return View(model);
		}

		public ActionResult DisplayForms(long id, bool edit = false)
		{
			var familyName = _BD_.drugfamily.Single(x => x.FamilyId == id).FamilyName;
			ViewData["familyName"] = familyName;
			ViewData["familyId"] = id;
			// формы данного лек.средства из ассортимента данного производителя
			var model = _BD_.drugformproducer.Where(x => x.DrugFamilyId == id && x.ProducerId == producerId).OrderBy(x => x.CatalogName).ToList();
			if (edit)
				return View("EditForms", model);
			return View(model);
		}

		[HttpPost]
		public ActionResult EditForms(long familyId)
		{
			var familyName = _BD_.drugfamily.Single(x => x.FamilyId == familyId).FamilyName;
			ViewData["familyName"] = familyName;
			ViewData["familyId"] = familyId;
			// формы данного лек.средства из ассортимента данного производителя
			var model = _BD_.drugformproducer.Where(x => x.DrugFamilyId == familyId && x.ProducerId == producerId).OrderBy(x => x.CatalogName).ToList();

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
			var df = _BD_.drugfamily.Single(x => x.FamilyId == familyId);
			drugdescription model;
      if (df.DescriptionId == null) {
				model = new drugdescription();
	      _BD_.drugdescription.Add(model);
				//_BD_.SaveChanges();
	      df.DescriptionId = model.DescriptionId;
				//_BD_.SaveChanges();
			}
			else
				model = _BD_.drugdescription.Single(x => x.DescriptionId == df.DescriptionId);

			var propertyInfo = model.GetType().GetProperty(field);
			propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			// TODO где-то сохранять
			//_BD_.SaveChanges();
			return Json(new { field = field, value = value });
		}

		public JsonResult EditMnnField(long familyId, string field, string value)
		{
			var df = _BD_.drugfamily.Single(x => x.FamilyId == familyId);
			drugmnn model;
			if (df.MnnId == null)
			{
				model = new drugmnn();
				_BD_.drugmnn.Add(model);
				//_BD_.SaveChanges();
				df.MnnId = model.MnnId;
				//_BD_.SaveChanges();
			}
			else
				model = _BD_.drugmnn.Single(x => x.MnnId == df.MnnId);

			var propertyInfo = model.GetType().GetProperty(field);
			propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));
			// TODO где-то сохранять
			//_BD_.SaveChanges();
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