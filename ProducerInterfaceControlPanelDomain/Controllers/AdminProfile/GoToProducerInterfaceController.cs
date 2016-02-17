using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers.AdminProfile
{
    public class GoToProducerInterfaceController : MasterBaseController
    {
   
        private ProducerInterfaceCommon.Heap.NamesHelper h;

        // GET: GoToProducerInterface
        [HttpGet]
        public ActionResult Index()
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
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

            var i = G1.ToString().Replace("-","").Replace(" ","") +  (CurrentUser.Name.Length * 1980).ToString() + G2.ToString().Replace("-", "").Replace(" ", "");
            var Url = GetWebConfigParameters("GoToProducerUserUrl");

            return Redirect(Url + "?SecureHash=" + i + "&AdminLogin=" + CurrentUser.Login + "&IdProducerUSer=" + produceruserid);            
        }

        public JsonResult GetListUser(long idproducer)
        {
            ProducerInterfaceCommon.Heap.NamesHelper h = new ProducerInterfaceCommon.Heap.NamesHelper(cntx_, CurrentUser.Id);
            var UserList = h.GetProducerUserList(idproducer);
            return Json(UserList.Select(x=> new { value=x.Value, text= x.Text}), JsonRequestBehavior.AllowGet);
        }


  

    }
}