using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnalitFramefork.Mvc;
using NHibernate.Linq;
using ProducerInterface.Models;

namespace ProducerInterface.Controllers
{
	public class HomeController : BaseInterfaceController
	{
		//
		// GET: /Главная/
		public ActionResult Index()
		{
			ViewBag.CurrentUser = DbSession.Query<User>().FirstOrDefault(e => e.Name == User.Identity.Name);
			var producers = DbSession.Query<Producer>().ToList();
			ViewBag.Producers = producers;
			return View();
		}
	}
}