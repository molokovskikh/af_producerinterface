using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceControlPanelDomain.Controllers.Global;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
    public class GoToProducerInterfaceController : BaseController
    {

        // GET: GoToProducerInterface
        [HttpGet]
        public ActionResult Index()
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(DB, CurrentUser.Id);
            var ProducerList = new List<OptionElement>() { new OptionElement { Text = "", Value = "" } };
            var ProducerListRegistration = h.RegisterListProducer();
            ProducerList.AddRange(ProducerListRegistration);
            return View(ProducerList);
        }

        [HttpPost]
        public ActionResult Index(long idproducer, long? produceruserid)
        {
            if (produceruserid == null)
            {
                ErrorMessage("Не выбран пользователь");
                return RedirectToAction("index");
            }

            Guid G1 = Guid.NewGuid();
            Guid G2 = Guid.NewGuid();

            var match = System.Text.RegularExpressions.Regex.Match(G1.ToString(), @"\-?\d+(\.\d{0,})?");
            var match2 = System.Text.RegularExpressions.Regex.Match(G2.ToString(), @"[0-9][0-9]+(?:\.[0-9]*)?");

            var ThisAccount = DB.Account.Find(CurrentUser.Id);
            ThisAccount.SecureTime = DateTime.Now.AddMinutes(5);

            DB.Entry(ThisAccount).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();

            if (CurrentUser.Name != null)
            {
                var i = match + (CurrentUser.Name.Length * 19801112).ToString() + match2;
                var Url = GetWebConfigParameters("GoToProducerUserUrl");
                var UrlRedirect = Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" + produceruserid;
                return Redirect(UrlRedirect);
            }
            else
            {
                var i = match + (18 * 19801112).ToString() + match2;
                var Url = GetWebConfigParameters("GoToProducerUserUrl");
                var UrlRedirect = Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" + produceruserid;
                return Redirect(UrlRedirect);
            }
        }

        public JsonResult GetListUser(long idproducer)
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(DB, CurrentUser.Id);
            var UserList = h.GetProducerUserList(idproducer);
            return Json(UserList.Select(x=> new { value=x.Value, text= x.Text}), JsonRequestBehavior.AllowGet);
        }
    }
}