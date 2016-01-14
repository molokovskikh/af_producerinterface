using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using System.Collections;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            IEnumerable<string> X = Groups().ToList();
          ViewBag.ListGroup = X;
            return View(X);
        }

        public List<string> Groups()
        {
            List<string> groups = new List<string>();

            foreach (IdentityReference group in System.Web.HttpContext.Current.Request.LogonUserIdentity.Groups)
            {
                groups.Add(group.Translate(typeof(NTAccount)).ToString().Replace("\\", ""));
            }

            return groups;
        }


    }
}