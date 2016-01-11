using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class PermissionController : pruducercontroller.BaseController
    {
        // GET: Permission


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }


        public ActionResult Index()
        {
            var ListUser = _BD_.produceruser.Where(xxx => xxx.Enabled == 1 && xxx.ProducerId == AutorizedUser.ProducerId).ToList();

            if (ListUser.Count() > 0)
            {
                ViewBag.UserList = ListUser;
            }
         
            return View();
        }
        public ActionResult Change(long? ID)
        {

            ViewBag.SelectUser = _BD_.produceruser.Where(xxx => xxx.Id == ID).First();

            return View();
        }

    }
}