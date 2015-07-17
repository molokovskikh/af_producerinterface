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
    public class HomeController : BaseController
    {
        //
        // GET: /Main/

        public ActionResult Index()
        {
			
	        var producers = DbSession.Query<Producer>().ToList();
	        ViewBag.Producers = producers;
            return View();
        }		
    }
}
