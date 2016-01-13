using Quartz.Job;
using Quartz.Job.EDM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterface.Models;

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
			var model = _BD_.drugfamily
				.Where(x => x.ProducerId == producerId)
				.OrderBy(x => x.FamilyName)
				.ToList();
			return View(model);
			//return null;
		}

		public ActionResult EditDescription(long id)
		{
			//id = 9872;
			//// TODO: сейчас неправильная БД
			//var model = _BD_.drugdescriptionremark.SingleOrDefault(x => x.DrugFamilyId == id);
			//if (model == null) {
			//	model = new drugdescriptionremark();
			//	model.CreationDate = DateTime.Now;
			//	model.ModificationDate = DateTime.Now;
			//	model.DrugFamilyId = id;
			//	model.ProducerUserId = userId;
			//}
			//return View(model);
			return null;
		}

		public JsonResult EditField(long id, string field, string value)
		{
			////_BD_.Configuration.ProxyCreationEnabled = false;
			//var model = _BD_.drugdescriptionremark.Single(x => x.Id == id);
			//var propertyInfo = model.GetType().GetProperty(field);
			//propertyInfo.SetValue(model, Convert.ChangeType(value, propertyInfo.PropertyType));

			////model.PharmacologicalAction = "ff"; //"Ингибитор АПФ. Антигипертензивный препарат. Механизм действия связан с ингибированием активности АПФ, что приводит к подавлению образования ангиотензина II из ангиотензина I и к прямому уменьшению выделения альдостерона. Уменьшает деградацию брадикинина и правка";
			
			//_BD_.SaveChanges();
			//return Json(new { field = field, value = value });
			return null;
		}

		//public ActionResult CreateDrugDescriptionRemark(int id)
		//{
		//    var family = DbSession.Query<DrugFamily>().First(i => i.Id == id);
		//    var remark = new DrugDescriptionRemark(family);
		//    var mnns = DbSession.Query<MNN>().ToList();
		//    ViewBag.AvailibleMnn = mnns;
		//    ViewBag.DrugDescriptionRemark = remark;
		//    return View("CreateDrugDescriptionRemark");
		//}

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