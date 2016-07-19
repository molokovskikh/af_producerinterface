using ProducerInterfaceCommon.ContextModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProducerInterface.Controllers
{
    public class PermissionController : BaseController
    {
        //Get User List in This Producer
        [HttpGet]
        public ActionResult Index()
        {
            var listUser = DB.Account.Where(x => x.Enabled == (sbyte)UserStatus.Active && x.CompanyId == CurrentUser.CompanyId && x.TypeUser == (SByte)SbyteTypeUser).ToList();
            if (listUser.Any())
                ViewBag.UserList = listUser;

            return View();
        }

        [HttpGet]
        public ActionResult Change(long? ID)
        {

            var SelectUser = DB.Account.Where(xxx => xxx.Id == ID).First();
            if (SelectUser.CompanyId != CurrentUser.CompanyId || SelectUser.Id == CurrentUser.Id)
            {
                ErrorMessage("У вас нет прав редактировать группы данного пользователя");
                return RedirectToAction("Index");
            }
            SelectUser.ListSelectedPermission = DB.AccountGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == SbyteTypeUser && xxx.Account.Any(zzz=>zzz.Id == ID))
                .ToList().Select(xxx =>(long) xxx.Id).ToList();

            ViewBag.GroupList = DB.AccountGroup.Where(xxx => xxx.TypeGroup == SbyteTypeUser && xxx.Enabled == true).ToList().Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + " " + xxx.Description }).ToList();

            return View(SelectUser);
        }

        [HttpPost]
        public ActionResult Change(long? Id, List<long> ListSelectedPermission)
        {

            var SelectUser = DB.Account.Where(xxx => xxx.Id == Id).First();
            if (SelectUser.CompanyId != CurrentUser.CompanyId || SelectUser.Id == CurrentUser.Id)
            {
                ErrorMessage("У вас нет прав редактировать права данного пользователя");
            }
            SelectUser.ListSelectedPermission = DB.AccountGroup
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
                    var GroupItem = DB.AccountGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.Account.Remove(DB.Account.Where(xxx => xxx.Id == Id).First());
                    DB.SaveChanges();
                }
            }

            // Добавляем пользователя в группы

            //
            foreach (var ItemUserGroup in ListSelectedPermission)
            {
                int GroupId =(int) ItemUserGroup;
                bool GroupExsist = SelectUser.AccountGroup.Any(xxx => xxx.Id == GroupId);

                if (!GroupExsist)
                {
                    var GroupItem = DB.AccountGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.Account.Add(DB.Account.Where(xxx => xxx.Id == Id).First());
                    DB.SaveChanges();
                }
            }

            SuccessMessage("Изменения сохранены");
            return RedirectToAction("Index");
        }

    }
}