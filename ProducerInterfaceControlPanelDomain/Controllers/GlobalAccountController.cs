using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class GlobalAccountController : MasterBaseController
    {

        sbyte Type = (sbyte)ProducerInterfaceCommon.ContextModels.TypeUsers.ProducerUser;

        // GET: GlobalAccount
        public ActionResult Index()
        {
            var ListUsers = cntx_.Account.Where(xxx=>xxx.TypeUser == Type).ToList();
            return View(ListUsers);
        }

        [HttpGet]
        public ActionResult SuccessAccount(long Id)
        {
            var ModelAccount = cntx_.Account.Where(xxx => xxx.Id == Id).First();
            ViewBag.Group = new List<long>();

            ViewBag.GroupList = cntx_.AccountGroup.Where(xxx => xxx.Enabled == true && xxx.TypeGroup == Type).ToList().Select(xxx => new OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + "(" + xxx.Description + ")"}).ToList();


            return View(ModelAccount);            
        }

        [HttpPost]
        public ActionResult SuccessAccount(ProducerInterfaceCommon.ContextModels.Account userModel, List<long> Group)
        {
            var ModelAccount = cntx_.Account.Where(xxx => xxx.Id == userModel.Id).First();
            SuccessMessage("Пользователь добавлен, ему отправлено сообщение с паролем на почту");
            return View(ModelAccount);
        }

        [HttpGet]
        public ActionResult DeleteAccount(long Id)
        {
            SuccessMessage("Пока не реализовано");
            return RedirectToAction("Index");
        }

    }
}