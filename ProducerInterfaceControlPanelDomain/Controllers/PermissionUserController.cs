using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterfaceControlPanelDomain.Controllers
{
    public class PermissionUserController : BasePermissionController
    {    
        ProducerInterfaceCommon.ContextModels.TypeUsers TypeLoginUserThis { get { return (ProducerInterfaceCommon.ContextModels.TypeUsers)SbyteTypeUser_; } set { SbyteTypeUser_ = (SByte)value; } }
        protected SByte SbyteTypeUser_ { get; set; }

        public PermissionUserController()
        {
            FilterType = TypeUsers.ProducerUser;
        }
         
        [HttpGet]
        public ActionResult SearchUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SearchUser(string TextSearch = null)
        {
            if (String.IsNullOrEmpty(TextSearch))
            {
                ErrorMessage("Не заполнена строка поиска");
                return RedirectToAction("SearchUser");
            }

            ViewBag.ListUser_ProducerUser = cntx_.ProducerUser.Where(xxx => xxx.TypeUser == SbyteTypeUser_ && (xxx.Name.Contains(TextSearch) || xxx.Email.Contains(TextSearch))).ToList();
            
            return View("Result");
        }
        [HttpGet]
        public ActionResult Change(long? Id)
        {
            var SelectUser = cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First();
         
            SelectUser.ListPermissionTwo = cntx_.ControlPanelGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == SbyteTypeUser_ && xxx.ProducerUser.Any(zzz => zzz.Id == Id))
                .ToList().Select(xxx => (long)xxx.Id).ToList();

            ViewBag.GroupList = cntx_.ControlPanelGroup.Where(xxx => xxx.TypeGroup == SbyteTypeUser_ && xxx.Enabled == true).ToList().Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + " " + xxx.Description }).ToList();

            return View(SelectUser);       
        }

        [HttpPost]
        public ActionResult Change(long? Id, List<long> ListPermissionTwo)
        {

            var SelectUser = cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First();
                     
            SelectUser.ListPermissionTwo = cntx_.ControlPanelGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == SbyteTypeUser_)
                .ToList().Select(xxx => (long)xxx.Id).ToList();

            //ListSelectedPermission - список групп в которых состоит пользователь, по мнению БД
            //Удаляем пользователя из групп
            foreach (var ItemUserGroup in SelectUser.ListPermissionTwo)
            {
                // ID группы Это Int
                int GroupId = (int)ItemUserGroup;

                bool GroupExsist = ListPermissionTwo.Any(xxx => xxx == ItemUserGroup);

                if (!GroupExsist) // если в входящем списке группы нет, удаляем из БД
                {
                    var GroupItem = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.ProducerUser.Remove(cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First());
                    cntx_.SaveChanges();
                }
            }

            // Добавляем пользователя в группы

            // 
            foreach (var ItemUserGroup in ListPermissionTwo)
            {
                int GroupId = (int)ItemUserGroup;
                bool GroupExsist = SelectUser.ControlPanelGroup.Any(xxx => xxx.Id == GroupId);

                if (!GroupExsist)
                {
                    var GroupItem = cntx_.ControlPanelGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.ProducerUser.Add(cntx_.ProducerUser.Where(xxx => xxx.Id == Id).First());
                    cntx_.SaveChanges();
                }
            }

            SuccessMessage("Изменения сохранены");
            return RedirectToAction("Group");
        }


        public ActionResult SearchUserCompany(string TextSearch = null)
        {
            if (String.IsNullOrEmpty(TextSearch))
            {
                ErrorMessage("Не заполнена строка поиска");
                return RedirectToAction("SearchUser");
            }

            var X = cntx_.producernames.Where(zzz => zzz.ProducerName.Contains(TextSearch)).Select(zzz => zzz.ProducerId).FirstOrDefault();
            
            ViewBag.ListUser_ProducerUser = cntx_.ProducerUser.Where(xxx => xxx.ProducerId == X).ToList();
            
            return View("Result");
        }

   
    }
}