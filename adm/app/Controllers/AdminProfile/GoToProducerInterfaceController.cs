using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.Heap;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
	public class GoToProducerInterfaceController : BaseController
	{
		// GET: GoToProducerInterface
		[HttpGet]
		public ActionResult Index()
		{
			var h = new NamesHelper(CurrentUser.Id);
			var ProducerList = new List<OptionElement> {new OptionElement {Text = "<Без производителя>", Value = ""}};
			var ProducerListRegistration = h.RegisterListProducer();
			ProducerList.AddRange(ProducerListRegistration);
			return View(ProducerList);
		}

		[HttpPost]
		public ActionResult Index(long? produceruserid)
		{
			if (produceruserid == null) {
				ErrorMessage("Не выбран пользователь");
				return RedirectToAction("index");
			}

			var match = Regex.Match(Guid.NewGuid().ToString(), @"\-?\d+(\.\d{0,})?");
			var match2 = Regex.Match(Guid.NewGuid().ToString(), @"[0-9][0-9]+(?:\.[0-9]*)?");

			CurrentUser.SecureTime = DateTime.Now.AddMinutes(5);
			DB.SaveChanges();

			if (CurrentUser.Name != null) {
				var i = match + (CurrentUser.Name.Length*19801112).ToString() + match2;
				var Url = GetWebConfigParameters("GoToProducerUserUrl");
				var UrlRedirect = Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" +
					produceruserid;
				return Redirect(UrlRedirect);
			} else {
				var i = match + (18*19801112).ToString() + match2;
				var Url = GetWebConfigParameters("GoToProducerUserUrl");
				var UrlRedirect = Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" +
					produceruserid;
				return Redirect(UrlRedirect);
			}
		}

		public JsonResult GetListUser(long? idproducer)
		{
			var items = DB.Account.Where(x => x.AccountCompany.ProducerId == idproducer)
				.ToList();
			return Json(items.Select(x => new { text = x.Login + " " + x.Name, value = x.Id.ToString() }), JsonRequestBehavior.AllowGet);
		}
	}
}