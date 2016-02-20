using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public partial class ProducerInformationController : MasterBaseController
    {
        public ActionResult RequestAccount()
        {
            var AccountList = cntx_.Account.Where(xxx => xxx.Enabled == 0 && xxx.AccountCompany.ProducerId == null).ToList();
            return View(AccountList);
        }
    }
}