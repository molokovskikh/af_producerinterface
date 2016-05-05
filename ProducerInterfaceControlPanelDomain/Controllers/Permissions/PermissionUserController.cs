using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.ViewModel.ControlPanel.Permission;
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            FilterType = TypeUsers.ProducerUser;
            base.OnActionExecuting(filterContext);
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

            ViewBag.ListUser_ProducerUser = cntx_.Account.Where(xxx => xxx.TypeUser == FilterSbyte && (xxx.Name.Contains(TextSearch) || xxx.Login.Contains(TextSearch))).ToList();
            
            return View("Result");
        }

        public ActionResult Group()
        {
            var ListGroup = cntx_.AccountGroup.Where(xxx => xxx.TypeGroup == SbyteTypeUser_).ToList()
                .Select(xxx => new ListGroupView
                {
                    Id = xxx.Id,
                    ListUsersInGroup = cntx_.Account.Where(zzz => zzz.AccountGroup.Any(vvv => vvv.Id == xxx.Id)).ToList().Select(d => new UsersViewInChange { eMail = d.Login, Name = d.Name, ProducerName = d.AccountCompany.Name }).ToList(),
                    CountUser = xxx.Account.Where(eee => eee.Enabled == (sbyte)UserStatus.Active && eee.TypeUser == FilterSbyte).Count(),
                    NameGroup = xxx.Name,
                    Description = xxx.Description,
                    Users = xxx.Account.Where(zzz => zzz.TypeUser == SbyteTypeUser_ && zzz.Enabled == (sbyte)UserStatus.Active && zzz.TypeUser == FilterSbyte).Select(zzz => zzz.Name).ToArray(),
                    Permissions = xxx.AccountPermission.Where(zzz => zzz.Enabled == true && zzz.TypePermission == FilterSbyte).Select(zzz => zzz.ControllerAction + "  " + zzz.ActionAttributes).ToArray()
                });

            return View(ListGroup);
        }

        [HttpGet]
        public ActionResult Change(long? Id)
        {
            var SelectUser = cntx_.Account.Where(xxx => xxx.Id == Id).First();
         
            SelectUser.ListPermissionTwo = cntx_.AccountGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == FilterSbyte && xxx.Account.Any(zzz => zzz.Id == Id))
                .ToList().Select(xxx => (long)xxx.Id).ToList();

            ViewBag.GroupList = cntx_.AccountGroup.Where(xxx => xxx.TypeGroup == FilterSbyte && xxx.Enabled == true).ToList().Select(xxx => new ProducerInterfaceCommon.ContextModels.OptionElement { Value = xxx.Id.ToString(), Text = xxx.Name + " " + xxx.Description }).ToList();

            return View(SelectUser);       
        }

        [HttpPost]
        public ActionResult Change(long? Id, List<long> ListPermissionTwo)
        {

            var SelectUser = cntx_.Account.Where(xxx => xxx.Id == Id).First();
                     
            SelectUser.ListPermissionTwo = cntx_.AccountGroup
                .Where(xxx => xxx.Enabled == true && xxx.TypeGroup == FilterSbyte)
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
                    var GroupItem = cntx_.AccountGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.Account.Remove(cntx_.Account.Where(xxx => xxx.Id == Id).First());
                    cntx_.SaveChanges();
                }
            }

            // Добавляем пользователя в группы

            // 
            foreach (var ItemUserGroup in ListPermissionTwo)
            {
                int GroupId = (int)ItemUserGroup;
                bool GroupExsist = SelectUser.AccountGroup.Any(xxx => xxx.Id == GroupId);

                if (!GroupExsist)
                {
                    var GroupItem = cntx_.AccountGroup.Where(xxx => xxx.Id == GroupId).First();
                    GroupItem.Account.Add(cntx_.Account.Where(xxx => xxx.Id == Id).First());
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
            
            ViewBag.ListUser_ProducerUser = cntx_.Account.Where(xxx => xxx.AccountCompany.ProducerId == X).ToList();
            
            return View("Result");
        }

   
    }
}