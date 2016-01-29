using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class PermissionController : MasterBaseController
    {
      


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        //Get User List in This Producer
        [HttpGet]
        public ActionResult Index()
        {
            var ListUser = cntx_.ProducerUser.Where(xxx => xxx.Enabled == 1 && xxx.ProducerId == CurrentUser.ProducerId && xxx.TypeUser == (SByte)SbyteTypeUser).ToList();

            if (ListUser.Count() > 0)
            {
                ViewBag.UserList = ListUser;
            }

            return View();
        }

        [HttpGet]
        public ActionResult Change(long? ID)
        {

            var SelectUser = cntx_.ProducerUser.Where(xxx => xxx.Id == ID).First();
            if (SelectUser.ProducerId != CurrentUser.ProducerId || SelectUser.Id == CurrentUser.Id)
            {
                ErrorMessage("У вас нет прав редактировать группы данного пользователя");
                return RedirectToAction("Index");
            }
            SelectUser.ListSelectedPermission = cntx_.ControlPanelGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == SbyteTypeUser && xxx.ProducerUser.Any(zzz=>zzz.Id == ID))
                .ToList().Select(xxx =>(long) xxx.Id).ToList();

            ViewBag.GroupList = cntx_.ControlPanelGroup.Where(xxx => xxx.TypeGroup == SbyteTypeUser && xxx.Enabled == true).ToList().Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + " " + xxx.Description }).ToList();
                        
            return View(SelectUser);
        }

        [HttpPost]
        public ActionResult Change(long? Id, List<long> ListSelectedPermission)
        {

            var SelectUser = cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First();
            if (SelectUser.ProducerId != CurrentUser.ProducerId || SelectUser.Id == CurrentUser.Id)
            {
                ErrorMessage("У вас нет прав редактировать права данного пользователя");
            }
            SelectUser.ListSelectedPermission = cntx_.ControlPanelGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == SbyteTypeUser)
                .ToList().Select(xxx => (long)xxx.Id).ToList();
                   
            //ListSelectedPermission - список групп в которых состоит пользователь, по мнению БД
            //Удаляем пользователя из групп
            foreach (var ItemUserGroup in SelectUser.ListSelectedPermission)
            {
                // ID группы Это Int
                int GroupId = (int)ItemUserGroup;

                bool GroupExsist = ListSelectedPermission.Any(xxx => xxx == ItemUserGroup);

                if (!GroupExsist) // если в входящем списке группы нет, удаляем из БД
                {
                    var GroupItem = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.ProducerUser.Remove(cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First());
                    cntx_.SaveChanges();
                }
            }

            // Добавляем пользователя в группы

            // 
            foreach (var ItemUserGroup in ListSelectedPermission)
            {
                int GroupId =(int) ItemUserGroup;
                bool GroupExsist = SelectUser.ControlPanelGroup.Any(xxx => xxx.Id == GroupId);

                if (!GroupExsist)
                {
                    var GroupItem = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.ProducerUser.Add(cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First());
                    cntx_.SaveChanges();
                }
            }

            SuccessMessage("Изменения сохранены");
            return RedirectToAction("Index");
        }

    }
}